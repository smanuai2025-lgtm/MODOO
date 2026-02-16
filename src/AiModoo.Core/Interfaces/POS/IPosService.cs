using AiModoo.Core.Entities.POS;

namespace AiModoo.Core.Interfaces.POS;

public interface IPosService
{
    // Config
    Task<IEnumerable<PosConfig>> GetConfigsAsync(CancellationToken ct = default);
    Task<PosConfig?> GetConfigByIdAsync(int id, CancellationToken ct = default);

    // Sessions
    Task<IEnumerable<PosSession>> GetSessionsAsync(CancellationToken ct = default);
    Task<PosSession?> GetSessionByIdAsync(int id, CancellationToken ct = default);
    Task<PosSession> OpenSessionAsync(int configId, string userId, string userName, decimal openingBalance, CancellationToken ct = default);
    Task CloseSessionAsync(int sessionId, decimal closingBalance, string? notes, CancellationToken ct = default);

    // Orders
    Task<PosOrder?> GetOrderByIdAsync(int id, CancellationToken ct = default);
    Task<PosOrder> CreateOrderAsync(PosOrder order, CancellationToken ct = default);
    Task<IEnumerable<PosOrder>> GetSessionOrdersAsync(int sessionId, CancellationToken ct = default);
}
