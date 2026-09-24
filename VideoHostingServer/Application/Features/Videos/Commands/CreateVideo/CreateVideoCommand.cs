using Application.Models.Video;
using Application.Models.VideoProcessing;
using MediatR;

namespace Application.Features.Videos.Commands.CreateVideo;

public record CreateVideoCommand(VideoCreateModel Model) 
    : IRequest<VideoProcessingResult>;
