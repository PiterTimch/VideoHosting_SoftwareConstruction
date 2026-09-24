using Application.Interfaces;
using Application.Mappings;
using Application.Models.Search;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.SearchVideos;

public class SearchVideosQueryHandler(
        IGenericRepository<VideoEntity, long> repo,
        VideoMappingProfile mapper
    )
    : IRequestHandler<SearchVideosQuery, SearchResult<VideoItemModel>>
{
    public async Task<SearchResult<VideoItemModel>> Handle(SearchVideosQuery request, CancellationToken cancellationToken)
    {
        int currentPage = request.Model.Page < 1 ? 1 : request.Model.Page;
        int itemsPerPage = request.Model.ItemPerPage < 1 ? 10 : request.Model.ItemPerPage;

        IQueryable<VideoEntity> query = repo.AsQurable()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Video != "processing...");

        if (!string.IsNullOrWhiteSpace(request.Model.Q))
        {
            string q = request.Model.Q.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(q) ||
                x.Description.ToLower().Contains(q)
            );
        }

        if (!string.IsNullOrWhiteSpace(request.Model.Title))
        {
            string title = request.Model.Title.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(title));
        }

        if (int.TryParse(request.Model.CreateYearFrom, out int fromYear))
        {
            query = query.Where(x => x.DateCreated.Year >= fromYear);
        }

        if (int.TryParse(request.Model.CreateYearTo, out int toYear))
        {
            query = query.Where(x => x.DateCreated.Year <= toYear);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        string? sortBy = request.Model.SortBy?.ToLower();

        if (sortBy == "date")
        {
            query = query.OrderByDescending(x => x.DateCreated);
        }
        else if (sortBy == "views")
        {
            query = query.OrderByDescending(x => x.ViewCount);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var items = await mapper.ProjectToItemModel(
            query
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
        ).ToListAsync(cancellationToken);

        return new SearchResult<VideoItemModel>
        {
            Items = items,
            Pagination = new PaginationModel
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            }
        };
    }
}
