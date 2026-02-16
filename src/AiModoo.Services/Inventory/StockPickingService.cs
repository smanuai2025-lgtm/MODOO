using AiModoo.Core.Entities.Inventory;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Accounting;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Inventory;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Inventory;

public class StockPickingService : IStockPickingService
{
    private readonly IRepository<StockPicking> _pickingRepository;
    private readonly IRepository<StockPickingType> _pickingTypeRepository;
    private readonly IRepository<StockMove> _moveRepository;
    private readonly IStockQuantService _stockQuantService;
    private readonly IAccountingIntegrationService _accountingIntegration;
    private readonly IUnitOfWork _unitOfWork;

    public StockPickingService(
        IRepository<StockPicking> pickingRepository,
        IRepository<StockPickingType> pickingTypeRepository,
        IRepository<StockMove> moveRepository,
        IStockQuantService stockQuantService,
        IAccountingIntegrationService accountingIntegration,
        IUnitOfWork unitOfWork)
    {
        _pickingRepository = pickingRepository;
        _pickingTypeRepository = pickingTypeRepository;
        _moveRepository = moveRepository;
        _stockQuantService = stockQuantService;
        _accountingIntegration = accountingIntegration;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StockPicking>> GetAllAsync(PickingTypeCode? typeCode = null, StockPickingStatus? status = null, CancellationToken ct = default)
    {
        var query = _pickingRepository.Query()
            .Include(p => p.PickingType)
            .Include(p => p.SourceLocation)
            .Include(p => p.DestLocation)
            .Include(p => p.Moves).ThenInclude(m => m.Product)
            .AsQueryable();

        if (typeCode.HasValue)
            query = query.Where(p => p.PickingType.Code == typeCode.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        return await query.OrderByDescending(p => p.ScheduledDate).ToListAsync(ct);
    }

    public async Task<StockPicking?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _pickingRepository.Query()
            .Include(p => p.PickingType)
            .Include(p => p.SourceLocation)
            .Include(p => p.DestLocation)
            .Include(p => p.Moves).ThenInclude(m => m.Product).ThenInclude(p => p.Uom)
            .Include(p => p.Moves).ThenInclude(m => m.SourceLocation)
            .Include(p => p.Moves).ThenInclude(m => m.DestLocation)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<StockPicking> CreateAsync(StockPicking picking, CancellationToken ct = default)
    {
        var pickingType = await _pickingTypeRepository.GetByIdAsync(picking.PickingTypeId, ct)
            ?? throw new NotFoundException($"Picking type {picking.PickingTypeId} not found.");

        if (string.IsNullOrWhiteSpace(picking.Name))
        {
            var count = await _pickingRepository.CountAsync(ct);
            picking.Name = $"{pickingType.Sequence}{(count + 1):D5}";
        }

        if (picking.SourceLocationId == 0 && pickingType.DefaultSourceLocationId.HasValue)
            picking.SourceLocationId = pickingType.DefaultSourceLocationId.Value;

        if (picking.DestLocationId == 0 && pickingType.DefaultDestLocationId.HasValue)
            picking.DestLocationId = pickingType.DefaultDestLocationId.Value;

        if (picking.ScheduledDate == default)
            picking.ScheduledDate = DateTime.UtcNow;

        // Set move locations and status
        foreach (var move in picking.Moves)
        {
            move.SourceLocationId = picking.SourceLocationId;
            move.DestLocationId = picking.DestLocationId;
            move.Status = StockMoveStatus.Draft;
            move.Date = picking.ScheduledDate;
        }

        var created = await _pickingRepository.AddAsync(picking, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task ConfirmAsync(int id, CancellationToken ct = default)
    {
        var picking = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Picking {id} not found.");

        if (picking.Status != StockPickingStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only draft pickings can be confirmed.");

        picking.Status = StockPickingStatus.Ready;
        foreach (var move in picking.Moves)
            move.Status = StockMoveStatus.Ready;

        _pickingRepository.Update(picking);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ValidateAsync(int id, CancellationToken ct = default)
    {
        var picking = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Picking {id} not found.");

        if (picking.Status != StockPickingStatus.Ready && picking.Status != StockPickingStatus.Draft)
            throw new BusinessRuleException("INVALID_STATUS", "Only ready or draft pickings can be validated.");

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            picking.Status = StockPickingStatus.Done;
            picking.DoneDate = DateTime.UtcNow;

            var moveIds = new List<int>();

            foreach (var move in picking.Moves)
            {
                move.Status = StockMoveStatus.Done;
                move.Date = DateTime.UtcNow;
                move.UnitCost = move.Product.Cost;
                move.TotalCost = move.ProductUomQty * move.Product.Cost;

                // Update stock quants: decrease source, increase destination
                await _stockQuantService.UpdateQuantAsync(move.ProductId, move.SourceLocationId, -move.ProductUomQty, move.LotId, ct);
                await _stockQuantService.UpdateQuantAsync(move.ProductId, move.DestLocationId, move.ProductUomQty, move.LotId, ct);

                moveIds.Add(move.Id);
            }

            _pickingRepository.Update(picking);
            await _unitOfWork.SaveChangesAsync(ct);

            // Create accounting entries for inventory moves
            if (moveIds.Count > 0)
            {
                try
                {
                    await _accountingIntegration.PostInventoryMoveAsync(moveIds, ct);
                }
                catch
                {
                    // Log but don't fail the stock move if accounting fails
                }
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
        var picking = await GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Picking {id} not found.");

        if (picking.Status == StockPickingStatus.Done)
            throw new BusinessRuleException("CANNOT_CANCEL_DONE", "Cannot cancel a validated picking.");

        picking.Status = StockPickingStatus.Cancelled;
        foreach (var move in picking.Moves)
            move.Status = StockMoveStatus.Cancelled;

        _pickingRepository.Update(picking);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StockPickingType>> GetPickingTypesAsync(CancellationToken ct = default)
    {
        return await _pickingTypeRepository.Query()
            .Include(t => t.Warehouse)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Code)
            .ToListAsync(ct);
    }
}
