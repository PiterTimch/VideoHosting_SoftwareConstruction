using Application.Models.VideoProcessing;
using Application.Mappings;
using Domain.Entities.Video;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Jobs;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Features.Videos.Commands.UpdateVideo;

public class UpdateVideoHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper,
    IImageService imageService,
    IVideoFileService videoFileService,
    IBackgroundJobClient backgroundJobClient,
    IVideoProgressStore progressStore,
    ILogger<UpdateVideoHandler> logger
) : IRequestHandler<UpdateVideoCommand, VideoProcessingResult>
{
    public async Task<VideoProcessingResult> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        var entity = await repo.AsQurable()
            .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
            throw new Exception("Відео не знайдено");

        mapper.MapToEntity(model, entity);

        if (model.Image != null)
        {
            if (entity.Image != null)
                await imageService.DeleteImageAsync(entity.Image);

            entity.Image = await imageService.SaveImageAsync(model.Image);
        }

        var trackingId = Guid.NewGuid().ToString();

        if (model.Video != null)
        {
            if (entity.Video == "processing..." || entity.Video == "обробляється...")
            {
                logger.LogWarning(
                    "[VideoProgress] UpdateVideo rejected — video {VideoId} is already being processed",
                    entity.Id);
                throw new InvalidOperationException("Відео вже обробляється. Зачекайте завершення попереднього завантаження.");
            }

            if (entity.Video != null)
                await videoFileService.DeleteVideoAsync(entity.Video);

            entity.Video = "processing...";

            progressStore.Set(trackingId, new VideoProgressUpdate
            {
                Percentage = 0,
                Status = "В черзі",
                EstimatedTimeRemaining = "Розрахунок..."
            });

            logger.LogInformation(
                "[VideoProgress] UpdateVideo queued trackingId={TrackingId} videoId={VideoId}",
                trackingId,
                entity.Id);

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(model.Video.FileName)}");
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await model.Video.CopyToAsync(stream);
            }

            backgroundJobClient.Enqueue<VideoProcessingJob>(job => 
                job.ProcessVideoAsync(entity.Id, tempPath, trackingId));
        }

        await repo.SaveChangesAsync();

        return new VideoProcessingResult { TrackingId = trackingId };
    }
}
