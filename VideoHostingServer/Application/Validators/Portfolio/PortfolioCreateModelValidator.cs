using Application.Models.Portfolio;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Portfolio;
using FluentValidation;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Validators.Portfolio;

public class PortfolioCreateModelValidator : AbstractValidator<PortfolioCreateModel>
{
    public PortfolioCreateModelValidator(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(100).WithMessage("Назва повинна містити не більше 100 символів");

        RuleFor(x => x.NickName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Нікнейм є обов'язковим")
            .MaximumLength(100).WithMessage("Нікнейм повинен містити не більше 100 символів")
            .IsSlug()
            .UniquePropertyAsync<PortfolioCreateModel, PortfolioEntity, long>(db, nameof(PortfolioEntity.NickName), "Цей нікнейм вже зайнятий");

        RuleFor(x => x)
            .MustAsync(async (model, cancellation) =>
            {
                var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                    return true;

                return !await db.Portfolios.AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellation);
            })
            .WithMessage("У користувача вже є канал");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");
    }
}
