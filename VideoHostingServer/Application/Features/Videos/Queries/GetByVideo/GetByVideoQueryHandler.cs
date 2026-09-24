using Application.Interfaces;
using Application.Mappings;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetByVideo;

public class GetByVideoQueryHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper)
    : IRequestHandler<GetByVideoQuery, VideoItemModel>
{
    public async Task<VideoItemModel> Handle(GetByVideoQuery request, CancellationToken cancellationToken)
    {
        IQueryable<VideoEntity> query = repo.AsQurable().Where(x => !x.IsDeleted);

        if (request.Model.Id != null)
        {
            query = query.Where(x => x.Id == request.Model.Id.Value);
        }
        else if (!string.IsNullOrEmpty(request.Model.Slug))
        {
            query = query.Where(x => x.Slug == request.Model.Slug);
        }
        else
        {
            throw new Exception("Необхідно вказати Id або Slug");
        }

        var model = await mapper.ProjectToItemModel(query)
            .FirstOrDefaultAsync(cancellationToken);

        if (model == null)
            throw new Exception("Відео не знайдено");

        return model;
    }
}
