using Application.Models.Channel;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Channel;
using FluentValidation;

namespace Application.Validators.Channel;

public class ChannelDeleteModelValidator : AbstractValidator<ChannelDeleteModel>
{
    public ChannelDeleteModelValidator(AppDbContext db)
    {
        RuleFor(x => x.Id)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<ChannelDeleteModel, ChannelEntity, long>(db, "Канал не знайдено");
    }
}
