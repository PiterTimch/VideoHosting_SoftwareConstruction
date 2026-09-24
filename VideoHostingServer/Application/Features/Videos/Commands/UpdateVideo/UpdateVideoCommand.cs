using Application.Models.Video;
using Application.Models.VideoProcessing;
using MediatR;

namespace Application.Features.Videos.Commands.UpdateVideo;

public record UpdateVideoCommand(VideoUpdateModel Model)
    : IRequest<VideoProcessingResult>;
