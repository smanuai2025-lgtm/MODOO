using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class PaymentService : IPaymentService
{
    private readonly IRepository<Payment> _repository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<PaymentAllocation> _allocationRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IJournalEntryService _journalEntryService;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IRepository<Payment> repository,
        IRepository<Journal> journalRepository,
        IRepository<PaymentAllocation> allocationRepository,
        IRepository<Invoice> invoiceRepository,
        IJournalEntryService journalEntryService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _journalRepository = journalRepository;
        _allocationRepository = allocationRepository;
        _invoiceRepository = invoiceRepository;
        _journalEntryService = journalEntryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
        => await _repository.Query()
            .Include(p => p.Journal)
            .Include(p => p.PaymentMethod)
            .Include(p => p.Allocations)
            .OrderByDescending(p => p.Date)
            .ToListAsync(ct);

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repository.Query()
            .Include(p => p.Journal)
            .Include(p => p.PaymentMethod)
            .Include(p => p.JournalEntry)
            .Include(p => p.Allocations)
                .ThenInclude(a => a.Invoice)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken ct = default)
    {
        _ = await _journalRepository.GetByIdAsync(payment.JournalId, ct)
            ?? throw new NotFoundException(nameof(Journal), payment.JournalId);

        var count = await _repository.CountAsync(ct);
        payment.Number = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D5}";
        payment.Status = PaymentStatus.Draft;

        await _repository.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return payment;
    }

    public async Task PostAsync(int id, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Payment), id);
        if (payment.Status != PaymentStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft payments can be posted.");

        var journal = payment.Journal;
        var entry = new JournalEntry
        {
            JournalId = payment.JournalId,
            Date = payment.Date,
            Reference = payment.Number,
            Narration = payment.Memo ?? $"Payment {payment.Number}",
            SourceDocument = payment.Number,
            SourceModule = "Payment",
            Lines = new List<JournalEntryLine>()
        };

        int debitAccountId = journal.DefaultDebitAccountId ?? throw new BusinessRuleException("NO_DEFAULT_ACCOUNT", "Journal has no default debit account.");
        int creditAccountId = journal.DefaultCreditAccountId ?? throw new BusinessRuleException("NO_DEFAULT_ACCOUNT", "Journal has no default credit account.");

        entry.Lines.Add(new JournalEntryLine { AccountId = debitAccountId, Debit = payment.Amount, Credit = 0, PartnerId = payment.PartnerId, PartnerType = payment.PartnerType, Label = payment.Memo ?? payment.Number });
        entry.Lines.Add(new JournalEntryLine { AccountId = creditAccountId, Debit = 0, Credit = payment.Amount, PartnerId = payment.PartnerId, PartnerType = payment.PartnerType, Label = payment.Memo ?? payment.Number });

        var createdEntry = await _journalEntryService.CreateAsync(entry, ct);
        await _journalEntryService.PostAsync(createdEntry.Id, ct);

        payment.JournalEntryId = createdEntry.Id;
        payment.Status = PaymentStatus.Posted;
        _repository.Update(payment);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Payment), id);
        payment.Status = PaymentStatus.Cancelled;
        if (payment.JournalEntryId.HasValue)
            await _journalEntryService.CancelAsync(payment.JournalEntryId.Value, ct);
        _repository.Update(payment);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task AllocateToInvoiceAsync(int paymentId, int invoiceId, decimal amount, CancellationToken ct = default)
    {
        var payment = await GetByIdAsync(paymentId, ct)
            ?? throw new NotFoundException(nameof(Payment), paymentId);

        if (payment.Status != PaymentStatus.Posted)
            throw new BusinessRuleException("INVALID_STATUS", "Only posted payments can be allocated.");

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), invoiceId);

        if (invoice.Status != InvoiceStatus.Posted && invoice.Status != InvoiceStatus.PartiallyPaid)
            throw new BusinessRuleException("INVALID_STATUS", "Only posted or partially paid invoices can receive allocations.");

        if (amount <= 0)
            throw new BusinessRuleException("INVALID_AMOUNT", "Allocation amount must be greater than zero.");

        var unallocatedAmount = payment.Amount - payment.AllocatedAmount;
        if (amount > unallocatedAmount)
            throw new BusinessRuleException("EXCEEDS_UNALLOCATED", $"Allocation amount ({amount}) exceeds the payment's unallocated amount ({unallocatedAmount}).");

        if (amount > invoice.AmountDue)
            throw new BusinessRuleException("EXCEEDS_AMOUNT_DUE", $"Allocation amount ({amount}) exceeds the invoice's amount due ({invoice.AmountDue}).");

        var allocation = new PaymentAllocation
        {
            PaymentId = paymentId,
            InvoiceId = invoiceId,
            Amount = amount,
            AllocationDate = DateTime.Today
        };

        await _allocationRepository.AddAsync(allocation, ct);

        payment.AllocatedAmount += amount;
        _repository.Update(payment);

        invoice.AmountPaid += amount;
        invoice.AmountDue -= amount;
        invoice.Status = invoice.AmountDue <= 0 ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        _invoiceRepository.Update(invoice);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeallocateFromInvoiceAsync(int allocationId, CancellationToken ct = default)
    {
        var allocation = await _allocationRepository.GetByIdAsync(allocationId, ct)
            ?? throw new NotFoundException(nameof(PaymentAllocation), allocationId);

        var payment = await GetByIdAsync(allocation.PaymentId, ct)
            ?? throw new NotFoundException(nameof(Payment), allocation.PaymentId);

        var invoice = await _invoiceRepository.GetByIdAsync(allocation.InvoiceId, ct)
            ?? throw new NotFoundException(nameof(Invoice), allocation.InvoiceId);

        payment.AllocatedAmount -= allocation.Amount;
        _repository.Update(payment);

        invoice.AmountPaid -= allocation.Amount;
        invoice.AmountDue += allocation.Amount;
        invoice.Status = invoice.AmountPaid <= 0 ? InvoiceStatus.Posted : InvoiceStatus.PartiallyPaid;
        _invoiceRepository.Update(invoice);

        _allocationRepository.Delete(allocation);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
