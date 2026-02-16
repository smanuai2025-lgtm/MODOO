using AiModoo.Core.Constants;
using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Entities.POS;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using BusinessException = AiModoo.Core.Exceptions.BusinessRuleException;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using AiModoo.Core.Interfaces.POS;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.POS;

public class PosService : IPosService
{
    private readonly IRepository<PosConfig> _configRepository;
    private readonly IRepository<PosSession> _sessionRepository;
    private readonly IRepository<PosOrder> _orderRepository;
    private readonly IRepository<PosOrderLine> _orderLineRepository;
    private readonly IRepository<PosPayment> _paymentRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IStockQuantService _stockQuantService;
    private readonly IUnitOfWork _unitOfWork;

    public PosService(
        IRepository<PosConfig> configRepository,
        IRepository<PosSession> sessionRepository,
        IRepository<PosOrder> orderRepository,
        IRepository<PosOrderLine> orderLineRepository,
        IRepository<PosPayment> paymentRepository,
        IRepository<Product> productRepository,
        IRepository<Location> locationRepository,
        IStockQuantService stockQuantService,
        IUnitOfWork unitOfWork)
    {
        _configRepository = configRepository;
        _sessionRepository = sessionRepository;
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _paymentRepository = paymentRepository;
        _productRepository = productRepository;
        _locationRepository = locationRepository;
        _stockQuantService = stockQuantService;
        _unitOfWork = unitOfWork;
    }

    // ==================== Config ====================
    public async Task<IEnumerable<PosConfig>> GetConfigsAsync(CancellationToken ct = default)
    {
        return await _configRepository.Query()
            .Include(c => c.Warehouse)
            .Include(c => c.Journal)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<PosConfig?> GetConfigByIdAsync(int id, CancellationToken ct = default)
    {
        return await _configRepository.Query()
            .Include(c => c.Warehouse)
            .Include(c => c.StockLocation)
            .Include(c => c.Journal)
            .Include(c => c.Pricelist)
            .Include(c => c.DefaultTax)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    // ==================== Sessions ====================
    public async Task<IEnumerable<PosSession>> GetSessionsAsync(CancellationToken ct = default)
    {
        return await _sessionRepository.Query()
            .Include(s => s.Config)
            .OrderByDescending(s => s.OpeningDate)
            .ToListAsync(ct);
    }

    public async Task<PosSession?> GetSessionByIdAsync(int id, CancellationToken ct = default)
    {
        return await _sessionRepository.Query()
            .Include(s => s.Config).ThenInclude(c => c.Warehouse)
            .Include(s => s.Config).ThenInclude(c => c.StockLocation)
            .Include(s => s.Orders).ThenInclude(o => o.Lines).ThenInclude(l => l.Product)
            .Include(s => s.Orders).ThenInclude(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<PosSession> OpenSessionAsync(int configId, string userId, string userName, decimal openingBalance, CancellationToken ct = default)
    {
        var config = await _configRepository.GetByIdAsync(configId, ct)
            ?? throw new NotFoundException($"POS Config {configId} not found.");

        var existingOpen = await _sessionRepository.Query()
            .AnyAsync(s => s.ConfigId == configId && (s.Status == PosSessionStatus.Opening || s.Status == PosSessionStatus.Opened), ct);

        if (existingOpen)
            throw new BusinessException("POS_SESSION_EXISTS", "هناك جلسة مفتوحة بالفعل لنقطة البيع هذه | There is already an open session for this POS.");

        var sessionCount = await _sessionRepository.Query().CountAsync(s => s.ConfigId == configId, ct);
        var session = new PosSession
        {
            Name = $"{config.Name}/{(sessionCount + 1).ToString("D4")}",
            Status = PosSessionStatus.Opened,
            ConfigId = configId,
            UserId = userId,
            UserName = userName,
            OpeningDate = DateTime.UtcNow,
            OpeningBalance = openingBalance
        };

        await _sessionRepository.AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return session;
    }

    public async Task CloseSessionAsync(int sessionId, decimal closingBalance, string? notes, CancellationToken ct = default)
    {
        var session = await _sessionRepository.Query()
            .Include(s => s.Orders).ThenInclude(o => o.Payments)
            .Include(s => s.Orders).ThenInclude(o => o.Lines)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct)
            ?? throw new NotFoundException($"POS Session {sessionId} not found.");

        if (session.Status != PosSessionStatus.Opened)
            throw new BusinessException("SESSION_NOT_OPEN", "لا يمكن إغلاق جلسة غير مفتوحة | Cannot close a session that is not open.");

        session.Status = PosSessionStatus.Closed;
        session.ClosingDate = DateTime.UtcNow;
        session.ClosingBalance = closingBalance;
        session.Notes = notes;

        session.TotalSales = session.Orders.Where(o => !o.IsReturn && o.Status != PosOrderStatus.Cancelled).Sum(o => o.Total);
        session.TotalReturns = session.Orders.Where(o => o.IsReturn).Sum(o => o.Total);
        session.TotalTax = session.Orders.Where(o => o.Status != PosOrderStatus.Cancelled).Sum(o => o.TaxTotal);
        session.TotalDiscount = session.Orders.Where(o => o.Status != PosOrderStatus.Cancelled).Sum(o => o.DiscountTotal);
        session.OrderCount = session.Orders.Count(o => o.Status != PosOrderStatus.Cancelled);

        _sessionRepository.Update(session);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    // ==================== Orders ====================
    public async Task<PosOrder?> GetOrderByIdAsync(int id, CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Lines).ThenInclude(l => l.Product)
            .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .Include(o => o.Session).ThenInclude(s => s.Config)
            .Include(o => o.Partner)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<PosOrder> CreateOrderAsync(PosOrder order, CancellationToken ct = default)
    {
        var session = await _sessionRepository.Query()
            .Include(s => s.Config).ThenInclude(c => c.StockLocation)
            .Include(s => s.Config).ThenInclude(c => c.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == order.SessionId, ct)
            ?? throw new NotFoundException($"POS Session {order.SessionId} not found.");

        if (session.Status != PosSessionStatus.Opened)
            throw new BusinessException("SESSION_NOT_OPEN", "الجلسة غير مفتوحة | Session is not open.");

        var orderCount = await _orderRepository.Query().CountAsync(o => o.SessionId == session.Id, ct);
        order.Number = $"POS-{session.Name.Replace("/", "-")}-{(orderCount + 1).ToString("D4")}";
        order.OrderDate = DateTime.UtcNow;

        ComputeOrderTotals(order);

        order.Status = PosOrderStatus.Paid;

        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // ====== Integration: Inventory Deduction ======
        await DeductStockForOrderAsync(order, session, ct);

        return order;
    }

    public async Task<IEnumerable<PosOrder>> GetSessionOrdersAsync(int sessionId, CancellationToken ct = default)
    {
        return await _orderRepository.Query()
            .Include(o => o.Lines)
            .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .Where(o => o.SessionId == sessionId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    // ==================== Helpers ====================
    private void ComputeOrderTotals(PosOrder order)
    {
        decimal subTotal = 0, taxTotal = 0, discountTotal = 0;

        foreach (var line in order.Lines)
        {
            var lineSubTotal = line.Quantity * line.UnitPrice;
            var discountAmount = lineSubTotal * (line.Discount / 100m);
            line.SubTotal = lineSubTotal - discountAmount;
            line.Total = line.SubTotal + line.TaxAmount;

            subTotal += line.SubTotal;
            taxTotal += line.TaxAmount;
            discountTotal += discountAmount;
        }

        order.SubTotal = subTotal;
        order.TaxTotal = taxTotal;
        order.DiscountTotal = discountTotal;
        order.Total = subTotal + taxTotal;
        order.Change = order.AmountPaid > order.Total ? order.AmountPaid - order.Total : 0;
    }

    private async Task DeductStockForOrderAsync(PosOrder order, PosSession session, CancellationToken ct)
    {
        var locationId = session.Config.StockLocationId;
        if (!locationId.HasValue)
        {
            var defaultLocation = await _locationRepository.Query()
                .FirstOrDefaultAsync(l => l.WarehouseId == session.Config.WarehouseId && l.LocationType == LocationType.Internal, ct);
            locationId = defaultLocation?.Id;
        }

        if (!locationId.HasValue) return;

        foreach (var line in order.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId, ct);
            if (product == null) continue;

            if (product.Type == ProductType.Storable)
            {
                var quantityChange = order.IsReturn ? line.Quantity : -line.Quantity;
                await _stockQuantService.UpdateQuantAsync(line.ProductId, locationId.Value, quantityChange, null, ct);
            }
        }
    }
}
