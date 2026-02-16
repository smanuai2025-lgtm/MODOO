using AiModoo.Core.Entities.Sales;
using AiModoo.Core.Enums;

namespace AiModoo.Core.Interfaces.Sales;

public interface IPartnerService
{
    Task<IEnumerable<Partner>> GetAllAsync(PartnerType? type = null, CancellationToken ct = default);
    Task<Partner?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Partner> CreateAsync(Partner partner, CancellationToken ct = default);
    Task UpdateAsync(Partner partner, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Partner>> SearchAsync(string term, PartnerType? type = null, CancellationToken ct = default);
    Task<decimal> GetOutstandingBalanceAsync(int partnerId, CancellationToken ct = default);
}
