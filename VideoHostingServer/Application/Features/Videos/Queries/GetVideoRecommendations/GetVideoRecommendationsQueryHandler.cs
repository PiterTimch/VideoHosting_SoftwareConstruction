using Application.Interfaces;
using Application.Mappings;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideoRecommendations;

public class GetVideoRecommendationsQueryHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper,
    IVideoRecommendationService recommendationService)
    : IRequestHandler<GetVideoRecommendationsQuery, IEnumerable<VideoItemModel>>
{
    private const int MaxRecommendations = 10;

    public async Task<IEnumerable<VideoItemModel>> Handle(
        GetVideoRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var source = await repo.AsQurable()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == request.Model.VideoId && !v.IsDeleted, cancellationToken)
            ?? throw new Exception("Відео не знайдено");

        IQueryable<VideoEntity> candidatesQuery = repo.AsQurable()
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                     && v.Id != source.Id
                     && v.Video != "processing...");

        var candidates = await candidatesQuery.ToListAsync(cancellationToken);

        var topIds = candidates
            .Select(c => new { Entity = c, Score = recommendationService.ComputeScore(source, c) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(MaxRecommendations)
            .Select(x => x.Entity.Id)
            .ToHashSet();

        if (topIds.Count == 0)
            return [];

        return await mapper
            .ProjectToItemModel(
                repo.AsQurable()
                    .AsNoTracking()
                    .Where(v => topIds.Contains(v.Id))
            )
            .ToListAsync(cancellationToken);
    }
}
