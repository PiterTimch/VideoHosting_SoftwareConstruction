using Application.Interfaces;
using Application.Jobs;
using Application.Mappings;
using Application.Models.VideoProcessing;
using Domain.Entities.Video;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Videos.Commands.CreateVideo;

public class CreateVideoHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper,
    IImageService imageService,
    IVideoFileService videoFileService,
    IBackgroundJobClient backgroundJobClient,
    IVideoProgressStore progressStore,
    ILogger<CreateVideoHandler> logger
) : IRequestHandler<CreateVideoCommand, VideoProcessingResult>
{
    public async Task<VideoProcessingResult> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.MapToEntity(request.Model);

        if (request.Model.Image != null)
            entity.Image = await imageService.SaveImageAsync(request.Model.Image);

        entity.Video = "processing..."; 

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        var trackingId = Guid.NewGuid().ToString();

        progressStore.Set(trackingId, new VideoProgressUpdate
        {
            Percentage = 0,
            Status = "В черзі",
            EstimatedTimeRemaining = "Розрахунок..."
        });

        logger.LogInformation(
            "[VideoProgress] CreateVideo queued trackingId={TrackingId} videoId={VideoId}",
            trackingId,
            entity.Id);

        if (request.Model.Video != null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(request.Model.Video.FileName)}");
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await request.Model.Video.CopyToAsync(stream);
            }

            backgroundJobClient.Enqueue<VideoProcessingJob>(job => 
                job.ProcessVideoAsync(entity.Id, tempPath, trackingId));
        }

        return new VideoProcessingResult { TrackingId = trackingId };
    }
}
