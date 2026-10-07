using Application.Models.Portfolio;
using FluentValidation;

namespace Application.Validators.Portfolio;

public class PortfolioSearchModelValidator : AbstractValidator<PortfolioSearchModel>
{
    public PortfolioSearchModelValidator()
    {
    }
}
