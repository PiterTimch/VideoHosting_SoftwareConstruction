using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.Video;

public class VideoReactionModelValidator : AbstractValidator<VideoReactionModel>
{
    public VideoReactionModelValidator(AppDbContext db)
    {
        RuleFor(x => x.VideoId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("ID відео є обов'язковим")
            .MustExistAsync<VideoReactionModel, VideoEntity, long>(db, "Відео не знайдено");
    }
}
