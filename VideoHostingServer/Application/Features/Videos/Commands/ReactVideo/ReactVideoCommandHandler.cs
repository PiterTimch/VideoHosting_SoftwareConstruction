using Application.Interfaces;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Commands.ReactVideo;

public class ReactVideoCommandHandler(
    IGenericRepository<VideoReactionEntity, long> reactions,
    IGenericRepository<VideoEntity, long> videos,
    ICurrentUserService currentUser)
    : IRequestHandler<ReactVideoCommand>
{
    public async Task Handle(ReactVideoCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        var videoExists = await videos.AsQurable().AnyAsync(x => x.Id == request.Model.VideoId, cancellationToken);
        if (!videoExists)
            throw new KeyNotFoundException("Відео не знайдено.");

        var existing = await reactions.AsQurable()
            .FirstOrDefaultAsync(x => x.VideoId == request.Model.VideoId && x.UserId == userId, cancellationToken);

        if (existing is not null && existing.IsLike == request.Model.IsLike)
        {
            await reactions.DeleteAsync(existing.Id);
            return;
        }

        if (existing is not null)
        {
            existing.IsLike = request.Model.IsLike;
            await reactions.UpdateAsync(existing);
            return;
        }

        await reactions.AddAsync(new VideoReactionEntity
        {
            VideoId = request.Model.VideoId,
            UserId = userId,
            IsLike = request.Model.IsLike,
        });
        await reactions.SaveChangesAsync();
    }
}
