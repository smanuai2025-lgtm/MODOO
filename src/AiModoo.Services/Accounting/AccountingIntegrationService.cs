using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.POS;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Accounting;

public class AccountingIntegrationService : IAccountingIntegrationService
{
    private readonly IJournalEntryService _journalEntryService;
    private readonly IInvoiceService _invoiceService;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<StockMove> _stockMoveRepository;
    private readonly IRepository<InventoryAdjustment> _adjustmentRepository;
    private readonly IRepository<PosSession> _posSessionRepository;

    public AccountingIntegrationService(
        IJournalEntryService journalEntryService,
        IInvoiceService invoiceService,
        IRepository<Invoice> invoiceRepository,
        IRepository<Journal> journalRepository,
        IRepository<Account> accountRepository,
        IRepository<StockMove> stockMoveRepository,
        IRepository<InventoryAdjustment> adjustmentRepository,
        IRepository<PosSession> posSessionRepository)
    {
        _journalEntryService = journalEntryService;
        _invoiceService = invoiceService;
        _invoiceRepository = invoiceRepository;
        _journalRepository = journalRepository;
        _accountRepository = accountRepository;
        _stockMoveRepository = stockMoveRepository;
        _adjustmentRepository = adjustmentRepository;
        _posSessionRepository = posSessionRepository;
    }

    public async Task<JournalEntry> PostSalesInvoiceAsync(int invoiceId, CancellationToken ct = default)
    {
        // Delegates to InvoiceService.ConfirmAsync which creates the journal entry
        // This method is kept for cross-module integration compatibility
        var invoice = await _invoiceRepository.Query()
            .Include(i => i.JournalEntry)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct)
            ?? throw new NotFoundException($"Invoice {invoiceId} not found.");

        if (invoice.JournalEntryId.HasValue)
            return invoice.JournalEntry!;

        // If not yet confirmed, confirm it via InvoiceService
        await _invoiceService.ConfirmAsync(invoiceId, ct);

        var updated = await _invoiceRepository.Query()
            .Include(i => i.JournalEntry)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);

        return updated?.JournalEntry ?? throw new BusinessRuleException("ENTRY_NOT_CREATED", "Failed to create journal entry for invoice.");
    }

    public async Task<JournalEntry> PostPurchaseInvoiceAsync(int invoiceId, CancellationToken ct = default)
    {
        // Same as PostSalesInvoiceAsync - delegates to InvoiceService
        return await PostSalesInvoiceAsync(invoiceId, ct);
    }

    public async Task<JournalEntry> PostPaymentAsync(int paymentId, CancellationToken ct = default)
    {
        // Payments are already posted via PaymentService.PostAsync
        // This is for external modules that want to post payments through integration
        var entry = new JournalEntry
        {
            JournalId = 1,
            Date = DateTime.Now,
            Reference = $"PAY-{paymentId}",
            Narration = $"Payment #{paymentId}",
            SourceDocument = $"PAY-{paymentId}",
            SourceModule = "Payment",
            Lines = new List<JournalEntryLine>()
        };

        return await CreateAndPostEntryAsync(entry, ct);
    }

    public async Task<JournalEntry> PostInventoryMoveAsync(IEnumerable<int> stockMoveIds, CancellationToken ct = default)
    {
        var moves = await _stockMoveRepository.Query()
            .Include(m => m.Product)
            .Include(m => m.SourceLocation)
            .Include(m => m.DestLocation)
            .Where(m => stockMoveIds.Contains(m.Id))
            .ToListAsync(ct);

        if (moves.Count == 0)
            return new JournalEntry();

        var inventoryAccount = await GetAccountByCodeAsync(AccountCodes.Inventory, ct);
        var cogsAccount = await GetAccountByCodeAsync(AccountCodes.CostOfGoodsSold, ct);
        var stockInputAccount = await GetAccountByCodeAsync(AccountCodes.StockInput, ct);
        var stockOutputAccount = await GetAccountByCodeAsync(AccountCodes.StockOutput, ct);

        var journal = await _journalRepository.Query().FirstOrDefaultAsync(j => j.IsActive, ct)
            ?? throw new NotFoundException("No active journal found.");

        var lines = new List<JournalEntryLine>();
        decimal totalAmount = 0;

        foreach (var move in moves)
        {
            var amount = move.TotalCost > 0 ? move.TotalCost : move.ProductUomQty * move.Product.Cost;
            if (amount <= 0) continue;
            totalAmount += amount;

            var isIncoming = move.SourceLocation.LocationType != LocationType.Internal
                             && move.DestLocation.LocationType == LocationType.Internal;
            var isOutgoing = move.SourceLocation.LocationType == LocationType.Internal
                             && move.DestLocation.LocationType != LocationType.Internal;

            if (isIncoming)
            {
                // Receipt: Debit Inventory, Credit Stock Input
                lines.Add(new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = amount, Credit = 0, Label = $"Receipt: {move.Product.Name}" });
                lines.Add(new JournalEntryLine { AccountId = stockInputAccount.Id, Debit = 0, Credit = amount, Label = $"Receipt: {move.Product.Name}" });
            }
            else if (isOutgoing)
            {
                // Delivery: Debit COGS, Credit Inventory
                lines.Add(new JournalEntryLine { AccountId = cogsAccount.Id, Debit = amount, Credit = 0, Label = $"Delivery: {move.Product.Name}" });
                lines.Add(new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = 0, Credit = amount, Label = $"Delivery: {move.Product.Name}" });
            }
            // Internal transfers don't generate accounting entries
        }

        if (lines.Count == 0)
            return new JournalEntry();

        var entry = new JournalEntry
        {
            JournalId = journal.Id,
            Date = DateTime.UtcNow,
            Reference = $"STK-{string.Join(",", stockMoveIds.Take(3))}{(stockMoveIds.Count() > 3 ? "..." : "")}",
            Narration = $"Inventory Movement - {moves.Count} move(s), Total: {totalAmount:N2}",
            SourceDocument = moves.First().Picking != null ? $"PICK-{moves.First().PickingId}" : "STK-MOVE",
            SourceModule = "Inventory",
            Lines = lines
        };

        return await CreateAndPostEntryAsync(entry, ct);
    }

    public async Task<JournalEntry> PostInventoryAdjustmentAsync(int adjustmentId, CancellationToken ct = default)
    {
        var adjustment = await _adjustmentRepository.Query()
            .Include(a => a.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(a => a.Id == adjustmentId, ct)
            ?? throw new NotFoundException($"Adjustment {adjustmentId} not found.");

        var inventoryAccount = await GetAccountByCodeAsync(AccountCodes.Inventory, ct);
        var adjustmentExpenseAccount = await GetAccountByCodeAsync(AccountCodes.InventoryAdjustmentExpense, ct);

        var journal = await _journalRepository.Query().FirstOrDefaultAsync(j => j.IsActive, ct)
            ?? throw new NotFoundException("No active journal found.");

        var lines = new List<JournalEntryLine>();

        foreach (var line in adjustment.Lines)
        {
            var difference = line.RealQty - line.TheoreticalQty;
            if (difference == 0) continue;

            var amount = Math.Abs(difference) * line.Product.Cost;
            if (amount <= 0) continue;

            if (difference > 0)
            {
                // Gain: Debit Inventory, Credit Adjustment Expense
                lines.Add(new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = amount, Credit = 0, Label = $"Adj gain: {line.Product.Name}" });
                lines.Add(new JournalEntryLine { AccountId = adjustmentExpenseAccount.Id, Debit = 0, Credit = amount, Label = $"Adj gain: {line.Product.Name}" });
            }
            else
            {
                // Loss: Debit Adjustment Expense, Credit Inventory
                lines.Add(new JournalEntryLine { AccountId = adjustmentExpenseAccount.Id, Debit = amount, Credit = 0, Label = $"Adj loss: {line.Product.Name}" });
                lines.Add(new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = 0, Credit = amount, Label = $"Adj loss: {line.Product.Name}" });
            }
        }

        if (lines.Count == 0)
            return new JournalEntry();

        var entry = new JournalEntry
        {
            JournalId = journal.Id,
            Date = adjustment.AccountingDate ?? DateTime.UtcNow,
            Reference = $"ADJ-{adjustmentId}",
            Narration = $"Inventory Adjustment: {adjustment.Name}",
            SourceDocument = $"ADJ-{adjustmentId}",
            SourceModule = "Inventory",
            Lines = lines
        };

        return await CreateAndPostEntryAsync(entry, ct);
    }

    public async Task<JournalEntry> PostPosSessionAsync(int sessionId, CancellationToken ct = default)
    {
        var session = await _posSessionRepository.Query()
            .Include(s => s.Config).ThenInclude(c => c.Journal)
            .Include(s => s.Orders.Where(o => o.Status != PosOrderStatus.Cancelled))
                .ThenInclude(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .Include(s => s.Orders.Where(o => o.Status != PosOrderStatus.Cancelled))
                .ThenInclude(o => o.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException($"POS Session {sessionId} not found.");

        if (session.Orders.Count == 0)
            throw new BusinessRuleException("NO_ORDERS", "لا توجد طلبات في هذه الجلسة | No orders in this session.");

        var journal = session.Config.Journal
            ?? await GetJournalByCodeAsync("CSH", ct);

        var cashAccount = await GetAccountByCodeAsync(AccountCodes.Cash, ct);
        var salesAccount = await GetAccountByCodeAsync(AccountCodes.SalesRevenue, ct);
        var cogsAccount = await GetAccountByCodeAsync(AccountCodes.CostOfGoodsSold, ct);
        var inventoryAccount = await GetAccountByCodeAsync(AccountCodes.Inventory, ct);
        var taxAccount = await GetAccountByCodeAsync(AccountCodes.SalesTaxPayable, ct);

        var lines = new List<JournalEntryLine>();

        var totalSales = session.TotalSales;
        var totalTax = session.TotalTax;
        var totalCogs = session.Orders
            .Where(o => !o.IsReturn && o.Status != PosOrderStatus.Cancelled)
            .SelectMany(o => o.Lines)
            .Sum(l => l.Product?.Cost * l.Quantity ?? 0);

        // Debit: Cash / Bank with total received
        if (totalSales + totalTax > 0)
        {
            lines.Add(new JournalEntryLine
            {
                AccountId = cashAccount.Id,
                Debit = totalSales + totalTax,
                Credit = 0,
                Label = $"POS Sales - Session {session.Name}"
            });
        }

        // Credit: Sales Revenue
        if (totalSales > 0)
        {
            lines.Add(new JournalEntryLine
            {
                AccountId = salesAccount.Id,
                Debit = 0,
                Credit = totalSales,
                Label = $"POS Revenue - Session {session.Name}"
            });
        }

        // Credit: Tax Payable
        if (totalTax > 0)
        {
            lines.Add(new JournalEntryLine
            {
                AccountId = taxAccount.Id,
                Debit = 0,
                Credit = totalTax,
                Label = $"POS Tax - Session {session.Name}"
            });
        }

        // COGS: Debit COGS, Credit Inventory
        if (totalCogs > 0)
        {
            lines.Add(new JournalEntryLine
            {
                AccountId = cogsAccount.Id,
                Debit = totalCogs,
                Credit = 0,
                Label = $"COGS - POS Session {session.Name}"
            });
            lines.Add(new JournalEntryLine
            {
                AccountId = inventoryAccount.Id,
                Debit = 0,
                Credit = totalCogs,
                Label = $"Inventory - POS Session {session.Name}"
            });
        }

        var entry = new JournalEntry
        {
            JournalId = journal.Id,
            Date = session.ClosingDate ?? DateTime.UtcNow,
            Reference = $"POS-{session.Name}",
            Narration = $"POS Session: {session.Name} | Orders: {session.OrderCount}",
            SourceDocument = $"POS-{sessionId}",
            SourceModule = "POS",
            Lines = lines
        };

        return await CreateAndPostEntryAsync(entry, ct);
    }

    public async Task<JournalEntry> PostPayrollEntryAsync(int payslipId, CancellationToken ct = default)
    {
        // Will be fully implemented when HR module is built
        var entry = new JournalEntry
        {
            JournalId = 1,
            Date = DateTime.Now,
            Reference = $"PAY-{payslipId}",
            Narration = $"Payroll Entry - Payslip #{payslipId}",
            SourceDocument = $"PAYSLIP-{payslipId}",
            SourceModule = "HR",
            Lines = new List<JournalEntryLine>()
        };

        return await CreateAndPostEntryAsync(entry, ct);
    }

    public async Task ReverseEntryAsync(int journalEntryId, string reason, CancellationToken ct = default)
    {
        await _journalEntryService.ReverseAsync(journalEntryId, DateTime.UtcNow, ct);
    }

    private async Task<JournalEntry> CreateAndPostEntryAsync(JournalEntry entry, CancellationToken ct)
    {
        if (entry.Lines.Count == 0)
            return entry; // Skip empty entries (placeholder for future modules)

        var created = await _journalEntryService.CreateAsync(entry, ct);
        await _journalEntryService.PostAsync(created.Id, ct);
        return created;
    }

    private async Task<Account> GetAccountByCodeAsync(string code, CancellationToken ct)
    {
        var accounts = await _accountRepository.Query().Where(a => a.Code == code).ToListAsync(ct);
        return accounts.FirstOrDefault() ?? throw new NotFoundException($"Account with code '{code}' not found.");
    }

    private async Task<Journal> GetJournalByCodeAsync(string code, CancellationToken ct)
    {
        var journals = await _journalRepository.Query().Where(j => j.Code == code).ToListAsync(ct);
        return journals.FirstOrDefault() ?? throw new NotFoundException($"Journal with code '{code}' not found.");
    }
}
