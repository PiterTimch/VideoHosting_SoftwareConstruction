using Application.Interfaces;
using Application.Mappings;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetVideoPrivacies;

public class GetVideoPrivaciesQueryHandler(IGenericRepository<VideoPrivacyEntity, long> repo, VideoMappingProfile mapper)
    : IRequestHandler<GetVideoPrivaciesQuery, IEnumerable<VideoPrivacyItemModel>>
{
    public async Task<IEnumerable<VideoPrivacyItemModel>> Handle(GetVideoPrivaciesQuery request, CancellationToken cancellationToken)
    {
        return await mapper.ProjectToItemModel(repo.AsQurable())
            .ToListAsync(cancellationToken);
    }
}
