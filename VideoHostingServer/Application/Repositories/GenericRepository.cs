using Microsoft.EntityFrameworkCore;
using Domain;
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class GenericRepository<TEntity, TKey>(AppDbContext context, IImageService? imageService) :
    IGenericRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, new()
{
    public async Task<TEntity?> GetByIdAsync(TKey id, bool isSoft = false)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);

        if (entity == null)
            return null;

        return entity.IsDeleted == isSoft ? entity : null;
    }

    public async Task<IReadOnlyList<TEntity>> ListAllAsync(bool isSoft = false)
    {
        var query = context.Set<TEntity>().AsQueryable();
        if (!isSoft)
            query = query.Where(e => !e.IsDeleted);
        return await query
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec)
    {
        if (spec == null) throw new ArgumentNullException(nameof(spec));

        IQueryable<TEntity> query = spec.Includes.Aggregate(
            context.Set<TEntity>().AsQueryable(),
            (current, include) => current.Include(include));

        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        query = query.Where(e => !e.IsDeleted);

        if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }
        else if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else
        {
            query = query.OrderBy(e => e.Id);
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        await context.Set<TEntity>().AddAsync(entity);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TKey id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        if (entity == null) return;

        entity.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

    public IQueryable<TEntity> AsQurable(bool isSoft = false)
    {
        var query = context.Set<TEntity>().AsQueryable();
        if (!isSoft)
            query = query.Where(e => !e.IsDeleted);
        return query;
    }
}
