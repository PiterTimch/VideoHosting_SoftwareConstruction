using Application.Models.Channel;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Channel;
using FluentValidation;

namespace Application.Validators.Channel;

public class ChannelUpdateModelValidator : AbstractValidator<ChannelUpdateModel>
{
    public ChannelUpdateModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<ChannelUpdateModel, ChannelEntity, long>(db, "Канал не знайдено");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва є обов'язковою")
            .MaximumLength(100).WithMessage("Назва повинна містити не більше 100 символів");

        RuleFor(x => x.NickName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Нікнейм є обов'язковим")
            .MaximumLength(100).WithMessage("Нікнейм повинен містити не більше 100 символів")
            .IsSlug()
            .UniquePropertyUpdateAsync<ChannelUpdateModel, ChannelEntity, long>(db, nameof(ChannelEntity.NickName), x => x.Id, "Цей нікнейм вже зайнятий");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Опис повинен містити не більше 1000 символів");

        RuleFor(x => x.AvatarImage)
            .IsImage();

        RuleFor(x => x.BannerImage)
            .IsImage();
    }
}
