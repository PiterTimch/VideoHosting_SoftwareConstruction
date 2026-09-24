using Application.Interfaces;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.AutocompleteVideos;

public class AutocompleteVideosQueryHandler(
    IGenericRepository<VideoEntity, long> repo)
    : IRequestHandler<AutocompleteVideosQuery, IEnumerable<VideoAutocompleteModel>>
{
    public async Task<IEnumerable<VideoAutocompleteModel>> Handle(AutocompleteVideosQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Q))
            return [];

        string q = request.Q.Trim().ToLower();

        var query = repo.AsQurable()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Video != "processing...")
            .Where(x => x.Title.ToLower().Contains(q))
            .OrderBy(x => x.Title)
            .Take(10)
            .Select(x => new VideoAutocompleteModel
            {
                Title = x.Title,
                Slug = x.Slug,
                Image = x.Image
            });

        return await query.ToListAsync(cancellationToken);
    }
}
