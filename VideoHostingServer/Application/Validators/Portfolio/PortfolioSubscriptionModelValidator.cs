using Application.Models.Portfolio;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Portfolio;
using FluentValidation;

namespace Application.Validators.Portfolio;

public class PortfolioSubscriptionModelValidator : AbstractValidator<PortfolioSubscriptionModel>
{
    public PortfolioSubscriptionModelValidator(AppDbContext db)
    {
        RuleFor(x => x.PortfolioId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<PortfolioSubscriptionModel, PortfolioEntity, long>(db, "Канал не знайдено");
    }
}
