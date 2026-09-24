using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.Video;

public class VideoDeleteModelValidator : AbstractValidator<VideoDeleteModel>
{
    public VideoDeleteModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Id є обов'язковим")
            .MustExistAsync<VideoDeleteModel, VideoEntity, long>(db, "Відео не знайдено");
    }
}
