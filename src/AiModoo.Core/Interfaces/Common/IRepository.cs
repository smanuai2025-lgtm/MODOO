using System.Linq.Expressions;
using AiModoo.Core.Entities.Common;

namespace AiModoo.Core.Interfaces.Common;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    IQueryable<T> Query();
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}
