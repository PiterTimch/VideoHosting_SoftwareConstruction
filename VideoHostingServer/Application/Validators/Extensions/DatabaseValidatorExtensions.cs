using Domain;
using Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Application.Validators.Extensions;

public static class DatabaseValidatorExtensions
{
    public static IRuleBuilderOptions<T, TKey> MustExistAsync<T, TEntity, TKey>(
        this IRuleBuilder<T, TKey> ruleBuilder,
        AppDbContext db,
        string errorMessage = "Запис не знайдено")
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return ruleBuilder.MustAsync(async (id, cancellation) =>
            await db.Set<TEntity>().AnyAsync(e => e.Id.Equals(id) && !e.IsDeleted, cancellation))
            .WithMessage(errorMessage);
    }

    public static IRuleBuilderOptions<T, TKey?> MustExistAsync<T, TEntity, TKey>(
        this IRuleBuilder<T, TKey?> ruleBuilder,
        AppDbContext db,
        string errorMessage = "Запис не знайдено")
        where TEntity : class, IEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        return ruleBuilder.MustAsync(async (id, cancellation) =>
        {
            if (!id.HasValue) return true;
            return await db.Set<TEntity>().AnyAsync(e => e.Id.Equals(id.Value) && !e.IsDeleted, cancellation);
        })
        .WithMessage(errorMessage);
    }

    public static IRuleBuilderOptions<T, string> UniqueSlugAsync<T, TEntity, TKey>(
        this IRuleBuilder<T, string> ruleBuilder,
        AppDbContext db,
        string errorMessage = "Запис з таким слагом вже існує")
        where TEntity : class, IEntity<TKey>
    {
        return ruleBuilder.MustAsync(async (slug, cancellation) =>
        {
            if (string.IsNullOrWhiteSpace(slug)) return true;
            var normalized = slug.Trim().ToLower().Replace(" ", "-");
            return !await db.Set<TEntity>().AnyAsync(e => 
                EF.Property<string>(e, "Slug") == normalized && !e.IsDeleted, cancellation);
        })
        .WithMessage(errorMessage);
    }

    public static IRuleBuilderOptions<T, string> UniqueSlugUpdateAsync<T, TEntity, TKey>(
        this IRuleBuilder<T, string> ruleBuilder,
        AppDbContext db,
        Func<T, TKey> idSelector,
        string errorMessage = "Інший запис з таким слагом вже існує")
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        return ruleBuilder.MustAsync(async (model, slug, cancellation) =>
        {
            if (string.IsNullOrWhiteSpace(slug)) return true;
            var id = idSelector(model);
            var normalized = slug.Trim().ToLower().Replace(" ", "-");
            return !await db.Set<TEntity>().AnyAsync(e => 
                EF.Property<string>(e, "Slug") == normalized && 
                !e.Id.Equals(id) && 
                !e.IsDeleted, cancellation);
        })
        .WithMessage(errorMessage);
    }
}
