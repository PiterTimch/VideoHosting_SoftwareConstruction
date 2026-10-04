using Application.Models.Channel;
using FluentValidation;

namespace Application.Validators.Channel;

public class ChannelSearchModelValidator : AbstractValidator<ChannelSearchModel>
{
    public ChannelSearchModelValidator()
    {
    }
}
