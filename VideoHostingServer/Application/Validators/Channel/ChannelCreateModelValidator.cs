using Application.Models.Channel;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Channel;
using FluentValidation;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Validators.Channel;

public class ChannelCreateModelValidator : AbstractValidator<ChannelCreateModel>
{
    public ChannelCreateModelValidator(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(100).WithMessage("Назва повинна містити не більше 100 символів");

        RuleFor(x => x.NickName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Нікнейм є обов'язковим")
            .MaximumLength(100).WithMessage("Нікнейм повинен містити не більше 100 символів")
            .IsSlug()
            .UniquePropertyAsync<ChannelCreateModel, ChannelEntity, long>(db, nameof(ChannelEntity.NickName), "Цей нікнейм вже зайнятий");

        RuleFor(x => x)
            .MustAsync(async (model, cancellation) =>
            {
                var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                    return true;

                return !await db.Channels.AnyAsync(x => x.Id == userId && !x.IsDeleted, cancellation);
            })
            .WithMessage("У користувача вже є канал");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");
    }
}
