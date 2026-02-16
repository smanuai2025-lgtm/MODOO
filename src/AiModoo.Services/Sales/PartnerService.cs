using AiModoo.Core.Entities.Accounting;
using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;
using AiModoo.Core.Exceptions;
using AiModoo.Core.Interfaces.Common;
using AiModoo.Core.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace AiModoo.Services.Sales;

public class PartnerService : IPartnerService
{
    private readonly IRepository<Partner> _partnerRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartnerService(
        IRepository<Partner> partnerRepository,
        IRepository<Invoice> invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _partnerRepository = partnerRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Partner>> GetAllAsync(PartnerType? type = null, CancellationToken ct = default)
    {
        var query = _partnerRepository.Query()
            .Include(p => p.PaymentTerm)
            .Include(p => p.Pricelist)
            .Include(p => p.SalesOrders)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(p => p.PartnerType == type.Value);

        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Partner?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _partnerRepository.Query()
            .Include(p => p.PaymentTerm)
            .Include(p => p.Pricelist)
            .Include(p => p.AccountReceivable)
            .Include(p => p.AccountPayable)
            .Include(p => p.SalesOrders)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Partner> CreateAsync(Partner partner, CancellationToken ct = default)
    {
        var created = await _partnerRepository.AddAsync(partner, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return created;
    }

    public async Task UpdateAsync(Partner partner, CancellationToken ct = default)
    {
        _partnerRepository.Update(partner);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var partner = await _partnerRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Partner {id} not found.");
        _partnerRepository.Delete(partner);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Partner>> SearchAsync(string term, PartnerType? type = null, CancellationToken ct = default)
    {
        var query = _partnerRepository.Query()
            .Where(p => p.Name.Contains(term) ||
                        (p.NameAr != null && p.NameAr.Contains(term)) ||
                        (p.Email != null && p.Email.Contains(term)) ||
                        (p.Phone != null && p.Phone.Contains(term)));

        if (type.HasValue)
            query = query.Where(p => p.PartnerType == type.Value);

        return await query.OrderBy(p => p.Name).Take(20).ToListAsync(ct);
    }

    public async Task<decimal> GetOutstandingBalanceAsync(int partnerId, CancellationToken ct = default)
    {
        return await _invoiceRepository.Query()
            .Where(i => i.PartnerId == partnerId && i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.AmountDue, ct);
    }
}
