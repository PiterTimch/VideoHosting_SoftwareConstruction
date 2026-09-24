using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using FluentValidation;
using Application.Validators.Extensions;

namespace Application.Validators.Video;

public class VideoCreateModelValidator : AbstractValidator<VideoCreateModel>
{
    public VideoCreateModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(255).WithMessage("Назва повинна містити не більше 255 символів");

        RuleFor(x => x.Slug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Слаг є обов'язковим")
            .MaximumLength(255).WithMessage("Слаг повинен містити не більше 255 символів")
            .IsSlug()
            .UniqueSlugAsync<VideoCreateModel, VideoEntity, long>(db, "Відео з таким слагом вже існує");

        RuleFor(x => x.Video)
            .NotNull().WithMessage("Відео-файл є обов'язковим")
            .IsVideo();

        RuleFor(x => x.Description)
            .NotNull().WithMessage("Опис не може бути порожнім")
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");

        RuleFor(x => x.PrivacyId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Приватність є обов'язковою")
            .MustExistAsync<VideoCreateModel, VideoPrivacyEntity, long>(db, "Приватність не знайдено");

        RuleFor(x => x.Image)
            .IsImage();
    }
}
