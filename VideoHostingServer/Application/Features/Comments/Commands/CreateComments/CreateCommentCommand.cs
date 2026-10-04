using Application.Models.Comments;
using MediatR;

namespace Application.Features.Comments.Commands.CreateComments;

public record CreateCommentCommand(
    string Content,
    long VideoId,
    long? ParentId
) : IRequest<CommentsItemModal>;
