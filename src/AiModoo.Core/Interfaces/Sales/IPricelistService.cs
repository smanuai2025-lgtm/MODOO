using AiModoo.Core.Entities.Sales;

namespace AiModoo.Core.Interfaces.Sales;

public interface IPricelistService
{
    Task<IEnumerable<Pricelist>> GetAllAsync(CancellationToken ct = default);
    Task<Pricelist?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Pricelist> CreateAsync(Pricelist pricelist, CancellationToken ct = default);
    Task UpdateAsync(Pricelist pricelist, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<decimal> GetProductPriceAsync(int pricelistId, int productId, decimal qty = 1, CancellationToken ct = default);
}
