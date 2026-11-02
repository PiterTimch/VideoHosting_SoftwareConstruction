using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Commands.ReactVideo;

public record ReactVideoCommand(VideoReactionModel Model) : IRequest;
