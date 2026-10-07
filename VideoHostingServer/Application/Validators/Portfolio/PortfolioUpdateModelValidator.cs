using Application.Models.Portfolio;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Portfolio;
using FluentValidation;

namespace Application.Validators.Portfolio;

public class PortfolioUpdateModelValidator : AbstractValidator<PortfolioUpdateModel>
{
    public PortfolioUpdateModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<PortfolioUpdateModel, PortfolioEntity, long>(db, "Канал не знайдено");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(100).WithMessage("Назва повинна містити не більше 100 символів");

        RuleFor(x => x.NickName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Нікнейм є обов'язковим")
            .MaximumLength(100).WithMessage("Нікнейм повинен містити не більше 100 символів")
            .IsSlug()
            .UniquePropertyUpdateAsync<PortfolioUpdateModel, PortfolioEntity, long>(db, nameof(PortfolioEntity.NickName), x => x.Id, "Цей нікнейм вже зайнятий");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");

        RuleFor(x => x.AvatarImage)
            .IsImage();

        RuleFor(x => x.BannerImage)
            .IsImage();
    }
}
