using Application.Mappings;
using Application.Models.Comments;
using Application.Models.Search;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Queries.GetCommentsReplies;

public class GetCommentRepliesQueryHandler(AppDbContext context, CommentMappingProfile mapper)
    : IRequestHandler<GetCommentRepliesQuery, SearchResult<CommentsItemModal>>
{
    public async Task<SearchResult<CommentsItemModal>> Handle(
        GetCommentRepliesQuery request,
        CancellationToken cancellationToken
    )
    {
        int currentPage = request.Model.Page < 1 ? 1 : request.Model.Page;
        int itemsPerPage = request.Model.ItemPerPage < 1 ? 10 : request.Model.ItemPerPage;

        var query = context.Comments.AsNoTracking()
            .Where(x => x.ParentId == request.ParentId && !x.IsDeleted);

        int totalCount = await query.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        var pagedQuery = query
            .OrderBy(x => x.DateCreated)
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
