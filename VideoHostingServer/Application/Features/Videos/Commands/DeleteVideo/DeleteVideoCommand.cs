using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Commands.DeleteVideo;

public record DeleteVideoCommand(VideoDeleteModel Model) : IRequest;
