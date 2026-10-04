using Application.Models.Channel;
using Application.Validators.Extensions;
using Domain;
using Domain.Entities.Channel;
using FluentValidation;

namespace Application.Validators.Channel;

public class ChannelSubscriptionModelValidator : AbstractValidator<ChannelSubscriptionModel>
{
    public ChannelSubscriptionModelValidator(AppDbContext db)
    {
        RuleFor(x => x.ChannelId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Id повинен бути більше 0")
            .MustExistAsync<ChannelSubscriptionModel, ChannelEntity, long>(db, "Канал не знайдено");
    }
}
