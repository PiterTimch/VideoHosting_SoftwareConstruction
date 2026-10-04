using Application.Models.Account;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.Account;

public class AccountRegisterModelValidator : AbstractValidator<AccountRegisterModel>
{
    public AccountRegisterModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ім'я є обов'язковим");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email є обов'язковим")
            .EmailAddress().WithMessage("Некоректний Email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль є обов'язковим")
            .MinimumLength(6).WithMessage("Пароль повинен бути не менше 6 символів");

        RuleFor(x => x.ImageFile)
            .IsImage();
    }
}
