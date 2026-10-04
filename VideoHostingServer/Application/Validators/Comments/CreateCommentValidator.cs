using Application.Features.Comments.Commands.CreateComments;
using FluentValidation;

namespace Application.Validators.Comments;

public class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Текст коментаря обов'язковий")
            .MaximumLength(2000)
            .WithMessage("Коментар занадто довгий");

        RuleFor(x => x.VideoId).GreaterThan(0);
    }
}
