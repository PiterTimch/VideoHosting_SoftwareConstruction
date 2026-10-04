using Application.Models.Comments;
using Application.Models.Search;
using MediatR;

namespace Application.Features.Comments.Queries.GetCommentsReplies;

public record GetCommentRepliesQuery(long ParentId, BaseSearchParamsModel Model) : IRequest<SearchResult<CommentsItemModal>>;
