using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Comments;
using Application.Models.Search;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries.GetVideoComments;

public class GetVideoCommentsQueryHandler(
    AppDbContext context,
    CommentMappingProfile mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<GetVideoCommentsQuery, SearchResult<CommentsItemModal>>
{
    public async Task<SearchResult<CommentsItemModal>> Handle(
        GetVideoCommentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var videoExists = await context.Videos
            .Where(x => x.Id == request.VideoId && !x.IsDeleted)
            .ForDirectAccess(currentUser)
            .AnyAsync(cancellationToken);

        if (!videoExists)
            throw new Exception("Відео не знайдено");

        int currentPage = request.Model.Page < 1 ? 1 : request.Model.Page;
        int itemsPerPage = request.Model.ItemPerPage < 1 ? 10 : request.Model.ItemPerPage;

        var query = context.Comments.AsNoTracking()
            .Where(x => x.VideoId == request.VideoId && x.ParentId == null && !x.IsDeleted);

        int totalCount = await query.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        var pagedQuery = query
            .OrderByDescending(x => x.DateCreated)
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage);

        var items = await mapper.ProjectToItemModel(pagedQuery)
            .ToListAsync(cancellationToken);

        return new SearchResult<CommentsItemModal>
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
