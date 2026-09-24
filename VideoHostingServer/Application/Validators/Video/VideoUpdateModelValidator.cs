using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.Video;

public class VideoUpdateModelValidator : AbstractValidator<VideoUpdateModel>
{
    public VideoUpdateModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Id є обов'язковим")
            .MustExistAsync<VideoUpdateModel, VideoEntity, long>(db, "Відео не знайдено");

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(255).WithMessage("Назва повинна містити не більше 255 символів");

        RuleFor(x => x.Slug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Слаг є обов'язковим")
            .MaximumLength(255).WithMessage("Слаг повинен містити не більше 255 символів")
            .IsSlug()
            .UniqueSlugUpdateAsync<VideoUpdateModel, VideoEntity, long>(db, x => x.Id, "Інше відео з таким слагом вже існує");

        RuleFor(x => x.Video)
            .IsVideo();

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");

        RuleFor(x => x.PrivacyId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Приватність є обов'язковою")
            .MustExistAsync<VideoUpdateModel, VideoPrivacyEntity, long>(db, "Приватність не знайдено");

        RuleFor(x => x.Image)
            .IsImage();
    }
}
