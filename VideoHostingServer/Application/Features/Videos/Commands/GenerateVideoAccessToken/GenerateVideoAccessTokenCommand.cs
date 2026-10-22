using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Commands.GenerateVideoAccessToken;

public class GenerateVideoAccessTokenCommand(long videoId) : IRequest<VideoAccessTokenResponse>
{
    public long VideoId { get; } = videoId;
}
