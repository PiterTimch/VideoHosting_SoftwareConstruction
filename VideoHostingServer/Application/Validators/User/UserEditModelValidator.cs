using Application.Models.User;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.User;

public class UserEditModelValidator : AbstractValidator<UserEditModel>
{
    public UserEditModelValidator()
    {
        RuleFor(x => x.Image)
            .IsImage();
    }
}
