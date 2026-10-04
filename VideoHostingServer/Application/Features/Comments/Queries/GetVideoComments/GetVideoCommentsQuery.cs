using Application.Models.Comments;
using Application.Models.Search;
using MediatR;

namespace Application.Features.Comments.Queries.GetVideoComments;

public record GetVideoCommentsQuery(long VideoId, BaseSearchParamsModel Model) : IRequest<SearchResult<CommentsItemModal>>;
