using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Sales;

public class SalesOrderService : ISalesOrderService
{
    private readonly IRepository<SalesOrder> _orderRepository;
    private readonly IRepository<Partner> _partnerRepository;
    private readonly IRepository<StockPickingType> _pickingTypeRepository;
    private readonly IStockPickingService _stockPickingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IAccountingIntegrationService _accountingIntegration;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Tax> _taxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SalesOrderService(
        IRepository<SalesOrder> orderRepository,
        IRepository<Partner> partnerRepository,
        IRepository<StockPickingType> pickingTypeRepository,
        IStockPickingService stockPickingService,
        IInvoiceService invoiceService,
        IAccountingIntegrationService accountingIntegration,
        IRepository<Journal> journalRepository,
        IRepository<Tax> taxRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _partnerRepository = partnerRepository;
        _pickingTypeRepository = pickingTypeRepository;
        _stockPickingService = stockPickingService;
        _invoiceService = invoiceService;
        _accountingIntegration = accountingIntegration;
        _journalRepository = journalRepository;
        _taxRepository = taxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SalesOrder>> GetAllAsync(CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Partner)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Invoice)
            .Include(o => o.DeliveryPicking)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<SalesOrder?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Partner)
            .Include(o => o.Pricelist)
            .Include(o => o.PaymentTerm)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Lines).ThenInclude(l => l.Uom)
            .Include(o => o.Lines).ThenInclude(l => l.Tax)
            .Include(o => o.Invoice)
            .Include(o => o.DeliveryPicking)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<SalesOrder> CreateAsync(SalesOrder order, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(order.Number))
        {
            var count = await _orderRepository.CountAsync(ct);
            order.Number = $"SO{(count + 1):D5}";
        }

        if (order.OrderDate == default)
            order.OrderDate = DateTime.UtcNow;

        await ComputeTotalsAsync(order, ct);

        var created = await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateAsync(SalesOrder order, CancellationToken ct = default)
    {
        await ComputeTotalsAsync(order, ct);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ConfirmAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Sales order {orderId} not found.");

        if (order.Status != SalesOrderStatus.Quotation && order.Status != SalesOrderStatus.QuotationSent)
            throw new BusinessRuleException("INVALID_STATUS", "Only quotations can be confirmed.");

        order.Status = SalesOrderStatus.SalesOrder;
        order.ConfirmationDate = DateTime.UtcNow;

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CreateDeliveryAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Sales order {orderId} not found.");

        if (order.Status != SalesOrderStatus.SalesOrder)
            throw new BusinessRuleException("INVALID_STATUS", "Order must be confirmed first.");

        if (order.DeliveryPickingId.HasValue)
            throw new BusinessRuleException("DELIVERY_EXISTS", "Delivery already created for this order.");

        // Find outgoing picking type
        var outPickingType = await _pickingTypeRepository.Query()
            .FirstOrDefaultAsync(t => t.Code == PickingTypeCode.Outgoing, ct)
            ?? throw new NotFoundException("Outgoing picking type not found.");

        // Create delivery picking from order lines
        var moves = order.Lines
            .Where(l => l.Product.Type == ProductType.Storable || l.Product.Type == ProductType.Consumable)
            .Select(l => new StockMove
            {
                ProductId = l.ProductId,
                ProductUomQty = l.Quantity,
                UomId = l.UomId ?? l.Product.UomId,
                Reference = $"SO: {order.Number} - {l.Description}",
            })
            .ToList();

        if (moves.Count == 0)
        {
            // Service-only order, mark lines as delivered
            foreach (var line in order.Lines)
                line.DeliveredQty = line.Quantity;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        var picking = new StockPicking
        {
            PickingTypeId = outPickingType.Id,
            ScheduledDate = order.DeliveryDate ?? DateTime.UtcNow,
            SourceDocument = order.Number,
            Moves = moves
        };

        var createdPicking = await _stockPickingService.CreateAsync(picking, ct);
        order.DeliveryPickingId = createdPicking.Id;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CreateInvoiceAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Sales order {orderId} not found.");

        if (order.Status != SalesOrderStatus.SalesOrder)
            throw new BusinessRuleException("INVALID_STATUS", "Order must be confirmed first.");

        if (order.InvoiceId.HasValue)
            throw new BusinessRuleException("INVOICE_EXISTS", "Invoice already created for this order.");

        // Find sales journal
        var salesJournal = await _journalRepository.Query()
            .FirstOrDefaultAsync(j => j.Type == JournalTypeEnum.Sale, ct)
            ?? throw new NotFoundException("Sales journal not found.");

        var invoiceLines = order.Lines.Select((l, i) => new InvoiceLine
        {
            Sequence = i + 1,
            ProductId = l.ProductId,
            ProductName = l.Product.Name,
            Description = l.Description,
            DescriptionAr = l.DescriptionAr,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Discount = l.Discount,
            TaxId = l.TaxId,
            TaxAmount = l.TaxAmount,
            SubTotal = l.SubTotal,
            Total = l.Total,
            AccountId = salesJournal.DefaultDebitAccountId ?? salesJournal.DefaultCreditAccountId ?? 0
        }).ToList();

        var invoice = new Invoice
        {
            InvoiceType = InvoiceType.CustomerInvoice,
            Date = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            PartnerId = order.PartnerId,
            PartnerType = PartnerType.Customer,
            PartnerName = order.Partner.Name,
            PartnerNameAr = order.Partner.NameAr,
            JournalId = salesJournal.Id,
            SubTotal = order.SubTotal,
            TaxTotal = order.TaxTotal,
            Total = order.Total,
            AmountDue = order.Total,
            SourceDocument = order.Number,
            SourceModule = "Sales",
            SalesOrderId = order.Id,
            Lines = invoiceLines
        };

        var createdInvoice = await _invoiceService.CreateAsync(invoice, ct);

        // Update order lines - mark as invoiced
        foreach (var line in order.Lines)
            line.InvoicedQty = line.Quantity;

        order.InvoiceId = createdInvoice.Id;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Sales order {orderId} not found.");

        if (order.Status == SalesOrderStatus.Done)
            throw new BusinessRuleException("CANNOT_CANCEL", "Cannot cancel a completed order.");

        order.Status = SalesOrderStatus.Cancelled;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ComputeTotalsAsync(SalesOrder order, CancellationToken ct = default)
    {
        decimal subTotal = 0;
        decimal taxTotal = 0;

        foreach (var line in order.Lines)
        {
            line.SubTotal = line.Quantity * line.UnitPrice * (1 - line.Discount / 100);

            if (line.TaxId.HasValue)
            {
                var tax = await _taxRepository.GetByIdAsync(line.TaxId.Value, ct);
                if (tax != null)
                    line.TaxAmount = line.SubTotal * tax.Rate / 100;
            }

            line.Total = line.SubTotal + line.TaxAmount;
            subTotal += line.SubTotal;
            taxTotal += line.TaxAmount;
        }

        order.SubTotal = subTotal;
        order.TaxTotal = taxTotal;
        order.Total = subTotal + taxTotal;
    }
}
