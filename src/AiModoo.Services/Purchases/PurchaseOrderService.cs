using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.Purchases;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.Purchases;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Purchases;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IRepository<PurchaseOrder> _orderRepository;
    private readonly IRepository<Partner> _partnerRepository;
    private readonly IRepository<StockPickingType> _pickingTypeRepository;
    private readonly IStockPickingService _stockPickingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Tax> _taxRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderService(
        IRepository<PurchaseOrder> orderRepository,
        IRepository<Partner> partnerRepository,
        IRepository<StockPickingType> pickingTypeRepository,
        IStockPickingService stockPickingService,
        IInvoiceService invoiceService,
        IRepository<Journal> journalRepository,
        IRepository<Tax> taxRepository,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _partnerRepository = partnerRepository;
        _pickingTypeRepository = pickingTypeRepository;
        _stockPickingService = stockPickingService;
        _invoiceService = invoiceService;
        _journalRepository = journalRepository;
        _taxRepository = taxRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Vendor)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Bill)
            .Include(o => o.ReceiptPicking)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Vendor)
            .Include(o => o.PaymentTerm)
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Lines).ThenInclude(l => l.Uom)
            .Include(o => o.Lines).ThenInclude(l => l.Tax)
            .Include(o => o.Bill)
            .Include(o => o.ReceiptPicking)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<PurchaseOrder> CreateAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(order.Number))
        {
            var count = await _orderRepository.CountAsync(ct);
            order.Number = $"PO{(count + 1):D5}";
        }

        if (order.OrderDate == default)
            order.OrderDate = DateTime.UtcNow;

        await ComputeTotalsAsync(order, ct);

        var created = await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateAsync(PurchaseOrder order, CancellationToken ct = default)
    {
        await ComputeTotalsAsync(order, ct);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ConfirmAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Purchase order {orderId} not found.");

        if (order.Status != PurchaseOrderStatus.RFQ && order.Status != PurchaseOrderStatus.RFQSent)
            throw new BusinessRuleException("INVALID_STATUS", "Only RFQs can be confirmed.");

        order.Status = PurchaseOrderStatus.PurchaseOrder;
        order.ConfirmationDate = DateTime.UtcNow;

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CreateReceiptAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Purchase order {orderId} not found.");

        if (order.Status != PurchaseOrderStatus.PurchaseOrder)
            throw new BusinessRuleException("INVALID_STATUS", "Order must be confirmed first.");

        if (order.ReceiptPickingId.HasValue)
            throw new BusinessRuleException("RECEIPT_EXISTS", "Receipt already created for this order.");

        // Find incoming picking type
        var inPickingType = await _pickingTypeRepository.Query()
            .FirstOrDefaultAsync(t => t.Code == PickingTypeCode.Incoming, ct)
            ?? throw new NotFoundException("Incoming picking type not found.");

        // Create receipt picking from order lines
        var moves = order.Lines
            .Where(l => l.Product.Type == ProductType.Storable || l.Product.Type == ProductType.Consumable)
            .Select(l => new StockMove
            {
                ProductId = l.ProductId,
                ProductUomQty = l.Quantity,
                UomId = l.UomId ?? l.Product.UomId,
                Reference = $"PO: {order.Number} - {l.Description}",
            })
            .ToList();

        if (moves.Count == 0)
        {
            foreach (var line in order.Lines)
                line.ReceivedQty = line.Quantity;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);
            return;
        }

        var picking = new StockPicking
        {
            PickingTypeId = inPickingType.Id,
            ScheduledDate = order.ReceiptDate ?? DateTime.UtcNow,
            SourceDocument = order.Number,
            PartnerName = order.Vendor.Name,
            PartnerType = PartnerType.Vendor,
            Moves = moves
        };

        var createdPicking = await _stockPickingService.CreateAsync(picking, ct);
        order.ReceiptPickingId = createdPicking.Id;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CreateBillAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Purchase order {orderId} not found.");

        if (order.Status != PurchaseOrderStatus.PurchaseOrder)
            throw new BusinessRuleException("INVALID_STATUS", "Order must be confirmed first.");

        if (order.BillId.HasValue)
            throw new BusinessRuleException("BILL_EXISTS", "Vendor bill already created for this order.");

        // Find purchase journal
        var purchaseJournal = await _journalRepository.Query()
            .FirstOrDefaultAsync(j => j.Type == JournalTypeEnum.Purchase, ct)
            ?? throw new NotFoundException("Purchase journal not found.");

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
            AccountId = purchaseJournal.DefaultDebitAccountId ?? purchaseJournal.DefaultCreditAccountId ?? 0
        }).ToList();

        var bill = new Invoice
        {
            InvoiceType = InvoiceType.VendorBill,
            Date = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            PartnerId = order.VendorId,
            PartnerType = PartnerType.Vendor,
            PartnerName = order.Vendor.Name,
            PartnerNameAr = order.Vendor.NameAr,
            JournalId = purchaseJournal.Id,
            SubTotal = order.SubTotal,
            TaxTotal = order.TaxTotal,
            Total = order.Total,
            AmountDue = order.Total,
            SourceDocument = order.Number,
            SourceModule = "Purchases",
            PurchaseOrderId = order.Id,
            Lines = invoiceLines
        };

        var createdBill = await _invoiceService.CreateAsync(bill, ct);

        // Update order lines as billed
        foreach (var line in order.Lines)
            line.BilledQty = line.Quantity;

        // Update product costs from purchase price
        foreach (var line in order.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId, ct);
            if (product != null && line.UnitPrice > 0)
            {
                product.Cost = line.UnitPrice;
                _productRepository.Update(product);
            }
        }

        order.BillId = createdBill.Id;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int orderId, CancellationToken ct = default)
    {
        var order = await GetByIdAsync(orderId, ct)
            ?? throw new NotFoundException($"Purchase order {orderId} not found.");

        if (order.Status == PurchaseOrderStatus.Done)
            throw new BusinessRuleException("CANNOT_CANCEL", "Cannot cancel a completed order.");

        order.Status = PurchaseOrderStatus.Cancelled;
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ComputeTotalsAsync(PurchaseOrder order, CancellationToken ct = default)
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
