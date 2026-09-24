using Domain.Entities;

namespace Application.Interfaces;

public interface IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, bool isSoft = false);
    Task<IReadOnlyList<TEntity>> ListAllAsync(bool isSoft = false);
    Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec);
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TKey id);
    Task<int> SaveChangesAsync();
    IQueryable<TEntity> AsQurable(bool isSoft = false);
}
