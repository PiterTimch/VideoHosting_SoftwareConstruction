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
                     && v.Video != "processing..."
                     && v.Privacy != null
                     && v.Privacy.SystemCode == "PUBLIC");

        var candidates = await candidatesQuery.ToListAsync(cancellationToken);

        var ranked = candidates
            .Select(c => new { Entity = c, Score = recommendationService.ComputeScore(source, c) })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Entity.ViewCount)
            .Take(MaxRecommendations)
            .ToArray();

        var topIds = ranked.Select(x => x.Entity.Id).ToHashSet();

        if (topIds.Count == 0)
            return [];

        var results = await mapper
            .ProjectToItemModel(
                repo.AsQurable()
                    .AsNoTracking()
                    .Where(v => topIds.Contains(v.Id))
            )
            .ToListAsync(cancellationToken);
        var orderedIds = ranked.Select(x => x.Entity.Id).ToArray();
        var resultById = results.ToDictionary(x => x.Id);
        return orderedIds.Where(resultById.ContainsKey).Select(id => resultById[id]).ToArray();
    }
}
