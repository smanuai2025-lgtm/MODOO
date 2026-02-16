using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Invoice> _repository;
    private readonly IRepository<InvoiceLine> _lineRepository;
    private readonly IRepository<PaymentAllocation> _allocationRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IJournalEntryService _journalEntryService;
    private readonly IFiscalPeriodService _fiscalPeriodService;
    private readonly ITaxService _taxService;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(
        IRepository<Invoice> repository,
        IRepository<InvoiceLine> lineRepository,
        IRepository<PaymentAllocation> allocationRepository,
        IRepository<Account> accountRepository,
        IJournalEntryService journalEntryService,
        IFiscalPeriodService fiscalPeriodService,
        ITaxService taxService,
        IRepository<Journal> journalRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _allocationRepository = allocationRepository;
        _accountRepository = accountRepository;
        _journalEntryService = journalEntryService;
        _fiscalPeriodService = fiscalPeriodService;
        _taxService = taxService;
        _journalRepository = journalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(InvoiceType? type = null, InvoiceStatus? status = null, CancellationToken ct = default)
    {
        var query = _repository.Query()
            .Include(i => i.Journal)
            .Include(i => i.Lines)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(i => i.InvoiceType == type.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.Id)
            .ToListAsync(ct);
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _repository.Query()
            .Include(i => i.Journal)
            .Include(i => i.FiscalPeriod)
            .Include(i => i.JournalEntry)
            .Include(i => i.Lines).ThenInclude(l => l.Account)
            .Include(i => i.Lines).ThenInclude(l => l.Tax)
            .Include(i => i.PaymentAllocations)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (invoice.Lines == null || invoice.Lines.Count == 0)
            throw new BusinessRuleException("NO_LINES", "Invoice must have at least one line.");

        // Calculate line amounts
        var sequence = 1;
        foreach (var line in invoice.Lines)
        {
            line.Sequence = sequence++;
            line.SubTotal = Math.Round(line.Quantity * line.UnitPrice - line.Discount, 2);

            line.TaxAmount = line.TaxId.HasValue
                ? await _taxService.CalculateTaxAmount(line.SubTotal, line.TaxId.Value, ct)
                : 0;

            line.Total = line.SubTotal + line.TaxAmount;
        }

        // Calculate invoice totals
        invoice.SubTotal = invoice.Lines.Sum(l => l.SubTotal);
        invoice.TaxTotal = invoice.Lines.Sum(l => l.TaxAmount);
        invoice.Total = invoice.Lines.Sum(l => l.Total);
        invoice.AmountPaid = 0;
        invoice.AmountDue = invoice.Total;

        // Generate number and set status
        invoice.Number = await GenerateNumberAsync(invoice.InvoiceType, ct);
        invoice.Status = InvoiceStatus.Draft;

        // Set fiscal period
        var period = await _fiscalPeriodService.GetPeriodForDateAsync(invoice.Date, ct);
        if (period != null)
            invoice.FiscalPeriodId = period.Id;

        await _repository.AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(invoice.Id, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoice.Id);

        if (existing.Status != InvoiceStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft invoices can be updated.");

        // Recalculate line amounts
        if (invoice.Lines != null && invoice.Lines.Count > 0)
        {
            var sequence = 1;
            foreach (var line in invoice.Lines)
            {
                line.Sequence = sequence++;
                line.SubTotal = Math.Round(line.Quantity * line.UnitPrice - line.Discount, 2);

                line.TaxAmount = line.TaxId.HasValue
                    ? await _taxService.CalculateTaxAmount(line.SubTotal, line.TaxId.Value, ct)
                    : 0;

                line.Total = line.SubTotal + line.TaxAmount;
            }

            invoice.SubTotal = invoice.Lines.Sum(l => l.SubTotal);
            invoice.TaxTotal = invoice.Lines.Sum(l => l.TaxAmount);
            invoice.Total = invoice.Lines.Sum(l => l.Total);
            invoice.AmountDue = invoice.Total - invoice.AmountPaid;
        }

        _repository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ConfirmAsync(int id, CancellationToken ct = default)
    {
        var invoice = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);

        if (invoice.Status != InvoiceStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft invoices can be confirmed.");

        // Determine journal code based on invoice type
        var journalCode = invoice.InvoiceType is InvoiceType.CustomerInvoice or InvoiceType.CustomerCreditNote
            ? "SAL"
            : "PUR";

        var journal = await _journalRepository.Query()
            .Where(j => j.Code == journalCode)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Journal with code '{journalCode}' not found.");

        // Resolve control account (Accounts Receivable or Accounts Payable)
        var isCustomerSide = invoice.InvoiceType is InvoiceType.CustomerInvoice or InvoiceType.CustomerCreditNote;
        var controlAccountCode = isCustomerSide ? AccountCodes.AccountsReceivable : AccountCodes.AccountsPayable;

        var controlAccount = await _accountRepository.Query()
            .Where(a => a.Code == controlAccountCode)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Account with code '{controlAccountCode}' not found.");

        // Determine debit/credit direction:
        //   CustomerInvoice:    Debit AR,  Credit line/tax accounts
        //   VendorBill:         Credit AP, Debit line/tax accounts
        //   CustomerCreditNote: Credit AR, Debit line/tax accounts  (reverse of CustomerInvoice)
        //   VendorDebitNote:    Debit AP,  Credit line/tax accounts (reverse of VendorBill)
        var controlIsDebit = invoice.InvoiceType is InvoiceType.CustomerInvoice or InvoiceType.VendorDebitNote;

        // Build journal entry
        var entry = new JournalEntry
        {
            JournalId = journal.Id,
            Date = invoice.Date,
            Reference = invoice.Number,
            Narration = $"Invoice {invoice.Number}",
            NarrationAr = $"فاتورة {invoice.Number}",
            SourceDocument = invoice.Number,
            SourceModule = "Invoice",
            Lines = new List<JournalEntryLine>()
        };

        var totalAbs = Math.Abs(invoice.Total);

        // Control account line (AR or AP)
        entry.Lines.Add(new JournalEntryLine
        {
            AccountId = controlAccount.Id,
            Debit = controlIsDebit ? totalAbs : 0,
            Credit = controlIsDebit ? 0 : totalAbs,
            PartnerId = invoice.PartnerId,
            PartnerType = invoice.PartnerType,
            Label = invoice.Number
        });

        // Revenue / expense + tax lines
        foreach (var line in invoice.Lines)
        {
            if (!line.AccountId.HasValue)
                throw new BusinessRuleException("MISSING_ACCOUNT", $"Invoice line '{line.Description}' has no account specified.");

            var lineSubTotalAbs = Math.Abs(line.SubTotal);

            entry.Lines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId.Value,
                Debit = controlIsDebit ? 0 : lineSubTotalAbs,
                Credit = controlIsDebit ? lineSubTotalAbs : 0,
                PartnerId = invoice.PartnerId,
                PartnerType = invoice.PartnerType,
                Label = line.Description
            });

            // Tax line
            if (line.TaxId.HasValue && line.TaxAmount != 0)
            {
                var taxAccountId = await GetTaxAccountIdAsync(line, invoice.InvoiceType, ct);
                var taxAmountAbs = Math.Abs(line.TaxAmount);

                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = taxAccountId,
                    Debit = controlIsDebit ? 0 : taxAmountAbs,
                    Credit = controlIsDebit ? taxAmountAbs : 0,
                    PartnerId = invoice.PartnerId,
                    PartnerType = invoice.PartnerType,
                    Label = line.Description
                });
            }
        }

        // Create and post the journal entry
        var createdEntry = await _journalEntryService.CreateAsync(entry, ct);
        await _journalEntryService.PostAsync(createdEntry.Id, ct);

        // Update invoice
        invoice.JournalEntryId = createdEntry.Id;
        invoice.Status = InvoiceStatus.Posted;
        invoice.AmountDue = invoice.Total - invoice.AmountPaid;

        _repository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var invoice = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Invoice), id);

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new BusinessRuleException("ALREADY_CANCELLED", "Invoice is already cancelled.");

        invoice.Status = InvoiceStatus.Cancelled;

        if (invoice.JournalEntryId.HasValue)
            await _journalEntryService.CancelAsync(invoice.JournalEntryId.Value, ct);

        _repository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<Invoice> CreateCreditNoteAsync(int invoiceId, CancellationToken ct = default)
    {
        var original = await GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        if (original.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new BusinessRuleException("INVALID_STATUS", "Cannot create credit note for draft or cancelled invoices.");

        var creditNoteType = original.InvoiceType == InvoiceType.CustomerInvoice
            ? InvoiceType.CustomerCreditNote
            : InvoiceType.VendorDebitNote;

        var creditNote = new Invoice
        {
            InvoiceType = creditNoteType,
            Date = DateTime.UtcNow,
            DueDate = DateTime.UtcNow,
            PartnerId = original.PartnerId,
            PartnerType = original.PartnerType,
            PartnerName = original.PartnerName,
            PartnerNameAr = original.PartnerNameAr,
            JournalId = original.JournalId,
            CurrencyId = original.CurrencyId,
            Reference = $"Reversal of {original.Number}",
            Notes = $"Credit note for invoice {original.Number}",
            NotesAr = $"إشعار دائن للفاتورة {original.Number}",
            SourceDocument = original.Number,
            SourceModule = "Invoice",
            ReversalOfId = original.Id,
            Lines = original.Lines.Select(l => new InvoiceLine
            {
                Description = l.Description,
                DescriptionAr = l.DescriptionAr,
                AccountId = l.AccountId,
                Quantity = -l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = -l.Discount,
                TaxId = l.TaxId,
                ProductId = l.ProductId,
                ProductName = l.ProductName
            }).ToList()
        };

        return await CreateAsync(creditNote, ct);
    }

    public async Task RegisterPaymentAsync(int invoiceId, int paymentId, decimal amount, CancellationToken ct = default)
    {
        var invoice = await GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        if (invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new BusinessRuleException("INVALID_STATUS", "Cannot register payment for draft or cancelled invoices.");

        var allocation = new PaymentAllocation
        {
            PaymentId = paymentId,
            InvoiceId = invoiceId,
            Amount = amount,
            AllocationDate = DateTime.UtcNow
        };

        await _allocationRepository.AddAsync(allocation, ct);

        invoice.AmountPaid += amount;
        invoice.AmountDue = invoice.Total - invoice.AmountPaid;

        invoice.Status = invoice.AmountDue == 0
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;

        _repository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RecalculateAmountsAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        var totalPaid = invoice.PaymentAllocations?.Sum(a => a.Amount) ?? 0;

        invoice.AmountPaid = totalPaid;
        invoice.AmountDue = invoice.Total - totalPaid;

        if (invoice.Status is not (InvoiceStatus.Draft or InvoiceStatus.Cancelled))
        {
            if (invoice.AmountDue == 0)
                invoice.Status = InvoiceStatus.Paid;
            else if (invoice.AmountPaid != 0)
                invoice.Status = InvoiceStatus.PartiallyPaid;
            else
                invoice.Status = InvoiceStatus.Posted;
        }

        _repository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<string> GenerateNumberAsync(InvoiceType type, CancellationToken ct = default)
    {
        var prefix = type switch
        {
            InvoiceType.CustomerInvoice => "INV",
            InvoiceType.VendorBill => "BILL",
            InvoiceType.CustomerCreditNote => "CN",
            InvoiceType.VendorDebitNote => "DN",
            _ => "INV"
        };

        var year = DateTime.Now.Year;

        var count = await _repository.Query()
            .Where(i => i.InvoiceType == type && i.Date.Year == year)
            .CountAsync(ct);

        var sequence = count + 1;

        return $"{prefix}-{year}-{sequence:D5}";
    }

    private async Task<int> GetTaxAccountIdAsync(InvoiceLine line, InvoiceType invoiceType, CancellationToken ct)
    {
        var isSalesSide = invoiceType is InvoiceType.CustomerInvoice or InvoiceType.CustomerCreditNote;

        // Use the tax entity's configured account when available
        if (line.Tax != null)
        {
            var taxAccountId = isSalesSide ? line.Tax.SalesAccountId : line.Tax.PurchaseAccountId;
            if (taxAccountId.HasValue)
                return taxAccountId.Value;
        }

        // Fallback to default tax accounts from AccountCodes
        var fallbackCode = isSalesSide
            ? AccountCodes.SalesTaxPayable
            : AccountCodes.PurchaseTaxReceivable;

        var account = await _accountRepository.Query()
            .Where(a => a.Code == fallbackCode)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Tax account with code '{fallbackCode}' not found.");

        return account.Id;
    }
}
