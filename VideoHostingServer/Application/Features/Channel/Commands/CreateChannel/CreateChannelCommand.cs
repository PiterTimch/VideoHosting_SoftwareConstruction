using Application.Models.Channel;
using MediatR;

namespace Application.Features.Channel.Commands.CreateChannel;

public record CreateChannelCommand(ChannelCreateModel Model) : IRequest<ChannelItemModel>;
