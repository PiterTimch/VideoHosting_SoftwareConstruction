using Application.Interfaces;
using Application.Jobs;
using Application.Mappings;
using Application.Models.VideoProcessing;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Videos.Commands.CreateVideo;

public class CreateVideoHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper,
    IImageService imageService,
    IVideoFileService videoFileService,
    IBackgroundJobClient backgroundJobClient,
    IVideoProgressStore progressStore,
    ILogger<CreateVideoHandler> logger,
    ICurrentUserService currentUser,
    IGenericRepository<PortfolioEntity, long> portfolios,
    UserManager<UserEntity> userManager
) : IRequestHandler<CreateVideoCommand, VideoProcessingResult>
{
    public async Task<VideoProcessingResult> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var entity = mapper.MapToEntity(request.Model);
        var ownerId = currentUser.TryGetCurrentUserId();
        if (ownerId.HasValue)
        {
            var portfolio = await portfolios.AsQurable().FirstOrDefaultAsync(x => x.Id == ownerId.Value, cancellationToken);
            if (portfolio is null)
            {
                var owner = await userManager.FindByIdAsync(ownerId.Value.ToString());
                if (owner is not null)
                {
                    var name = $"{owner.FirstName} {owner.LastName}".Trim();
                    portfolio = new PortfolioEntity
                    {
                        Id = owner.Id,
                        Freelancer = owner,
                        Name = string.IsNullOrWhiteSpace(name) ? owner.UserName ?? $"Автор {owner.Id}" : name[..Math.Min(name.Length, 100)],
                        NickName = $"author_{owner.Id}"
                    };
                    await portfolios.AddAsync(portfolio);
                    await portfolios.SaveChangesAsync();
                }
            }
            if (portfolio is not null)
                entity.PortfolioId = portfolio.Id;
        }

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
