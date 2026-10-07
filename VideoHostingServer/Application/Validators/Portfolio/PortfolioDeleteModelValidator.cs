using Application.Models.Portfolio;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Portfolio;
using FluentValidation;

namespace Application.Validators.Portfolio;

public class PortfolioDeleteModelValidator : AbstractValidator<PortfolioDeleteModel>
{
    public PortfolioDeleteModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<PortfolioDeleteModel, PortfolioEntity, long>(db, "Канал не знайдено");
    }
}
