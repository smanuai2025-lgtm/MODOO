using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Inventory;

public class InventoryAdjustmentService : IInventoryAdjustmentService
{
    private readonly IRepository<InventoryAdjustment> _adjustmentRepository;
    private readonly IRepository<StockQuant> _quantRepository;
    private readonly IStockQuantService _stockQuantService;
    private readonly IAccountingIntegrationService _accountingIntegration;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryAdjustmentService(
        IRepository<InventoryAdjustment> adjustmentRepository,
        IRepository<StockQuant> quantRepository,
        IStockQuantService stockQuantService,
        IAccountingIntegrationService accountingIntegration,
        IUnitOfWork unitOfWork)
    {
        _adjustmentRepository = adjustmentRepository;
        _quantRepository = quantRepository;
        _stockQuantService = stockQuantService;
        _accountingIntegration = accountingIntegration;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<InventoryAdjustment>> GetAllAsync(CancellationToken ct = default)
    {
        return await _adjustmentRepository.Query()
            .Include(a => a.Warehouse)
            .Include(a => a.Location)
            .Include(a => a.Lines).ThenInclude(l => l.Product)
            .OrderByDescending(a => a.Date)
            .ToListAsync(ct);
    }

    public async Task<InventoryAdjustment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _adjustmentRepository.Query()
            .Include(a => a.Warehouse)
            .Include(a => a.Location)
            .Include(a => a.Lines).ThenInclude(l => l.Product).ThenInclude(p => p.Uom)
            .Include(a => a.Lines).ThenInclude(l => l.Location)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<InventoryAdjustment> CreateAsync(InventoryAdjustment adjustment, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(adjustment.Name))
        {
            var count = await _adjustmentRepository.CountAsync(ct);
            adjustment.Name = $"ADJ/{DateTime.UtcNow:yyyyMM}/{(count + 1):D4}";
        }

        if (adjustment.Date == default)
            adjustment.Date = DateTime.UtcNow;

        var created = await _adjustmentRepository.AddAsync(adjustment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task StartCountAsync(int id, CancellationToken ct = default)
    {
        var adjustment = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Adjustment {id} not found.");

        if (adjustment.Status != InventoryAdjustmentStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft adjustments can be started.");

        // Fill theoretical quantities from current stock
        foreach (var line in adjustment.Lines)
        {
            var locationId = line.LocationId ?? adjustment.LocationId;
            if (locationId.HasValue)
            {
                line.TheoreticalQty = await _stockQuantService.GetOnHandAsync(line.ProductId, locationId.Value, ct);
            }
        }

        adjustment.Status = InventoryAdjustmentStatus.InProgress;
        _adjustmentRepository.Update(adjustment);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ValidateAsync(int id, CancellationToken ct = default)
    {
        var adjustment = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Adjustment {id} not found.");

        if (adjustment.Status != InventoryAdjustmentStatus.InProgress && adjustment.Status != InventoryAdjustmentStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only in-progress or draft adjustments can be validated.");

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var line in adjustment.Lines)
            {
                var locationId = line.LocationId ?? adjustment.LocationId;
                if (!locationId.HasValue) continue;

                var difference = line.RealQty - line.TheoreticalQty;
                if (difference != 0)
                {
                    await _stockQuantService.UpdateQuantAsync(line.ProductId, locationId.Value, difference, line.LotId, ct);
                }
            }

            adjustment.Status = InventoryAdjustmentStatus.Validated;
            adjustment.AccountingDate = DateTime.UtcNow;
            _adjustmentRepository.Update(adjustment);
            await _unitOfWork.SaveChangesAsync(ct);

            // Create accounting entry for the adjustment
            try
            {
                await _accountingIntegration.PostInventoryAdjustmentAsync(adjustment.Id, ct);
            }
            catch
            {
                // Log but don't fail the adjustment if accounting fails
            }

            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var adjustment = await _adjustmentRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Adjustment {id} not found.");

        if (adjustment.Status == InventoryAdjustmentStatus.Validated)
            throw new BusinessRuleException("CANNOT_CANCEL_VALIDATED", "Cannot cancel a validated adjustment.");

        adjustment.Status = InventoryAdjustmentStatus.Cancelled;
        _adjustmentRepository.Update(adjustment);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
