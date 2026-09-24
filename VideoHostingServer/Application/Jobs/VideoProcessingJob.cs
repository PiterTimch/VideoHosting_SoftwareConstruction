using Application.Interfaces;
using Application.Models.VideoProcessing;
using Domain.Entities.Video;
using Microsoft.AspNetCore.SignalR;
using Application.Hubs;
using Microsoft.Extensions.Logging;

namespace Application.Jobs;

public class VideoProcessingJob(
    IHubContext<VideoProgressHub> hubContext,
    IVideoFileService videoFileService,
    IGenericRepository<VideoEntity, long> repo,
    IVideoProgressStore progressStore,
    ILogger<VideoProcessingJob> logger
)
{
    public async Task ProcessVideoAsync(long videoId, string tempFilePath, string trackingId)
    {
        logger.LogInformation("Starting background video processing for VideoId: {VideoId}, TrackingId: {TrackingId}", videoId, trackingId);
        
        try
        {
            if (!File.Exists(tempFilePath))
            {
                logger.LogError("Temp file not found at path: {TempPath}", tempFilePath);
                throw new FileNotFoundException("Тимчасовий файл для обробки відео не знайдено", tempFilePath);
            }

            await SendProgressAsync(trackingId, new VideoProgressUpdate
            {
                Percentage = 0,
                Status = "Початок",
                EstimatedTimeRemaining = "Розрахунок..."
            });

            logger.LogInformation("Calling VideoFileService to process video...");
            
            var baseFileName = await videoFileService.SaveVideoWithProgressAsync(tempFilePath, (progress) => {
                progressStore.Set(trackingId, progress);
                _ = hubContext.Clients.Group(trackingId).SendAsync("ReceiveProgress", progress);
            });

            logger.LogInformation("Video processing finished. BaseFileName: {FileName}. Updating database...", baseFileName);

            var entity = await repo.GetByIdAsync(videoId);
            if (entity != null)
            {
                entity.Video = baseFileName;
                await repo.UpdateAsync(entity);
                await repo.SaveChangesAsync();
                logger.LogInformation("Database updated successfully for VideoId: {VideoId}", videoId);
            }
            else
            {
                logger.LogWarning("Video entity with Id {VideoId} not found in database after processing", videoId);
            }

            await SendProgressAsync(trackingId, new VideoProgressUpdate
            {
                Percentage = 100,
                Status = "Завершено",
                EstimatedTimeRemaining = "00:00:00"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing video for TrackingId: {TrackingId}", trackingId);
            
            await SendProgressAsync(trackingId, new VideoProgressUpdate
            {
                Percentage = 0,
                Status = $"Помилка: {ex.Message}",
                EstimatedTimeRemaining = "N/A"
            });
            
            throw;
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try 
                { 
                    File.Delete(tempFilePath); 
                    logger.LogInformation("Deleted temp file: {TempPath}", tempFilePath);
                } 
                catch (Exception deleteEx) 
                { 
                    logger.LogWarning(deleteEx, "Failed to delete temp file: {TempPath}", tempFilePath);
                }
            }
        }
    }

    private async Task SendProgressAsync(string trackingId, VideoProgressUpdate update)
    {
        progressStore.Set(trackingId, update);
        logger.LogInformation(
            "[VideoProgress] Job SendProgress trackingId={TrackingId} status={Status} percentage={Percentage}",
            trackingId,
            update.Status,
            update.Percentage);
        await hubContext.Clients.Group(trackingId).SendAsync("ReceiveProgress", update);
    }
}
