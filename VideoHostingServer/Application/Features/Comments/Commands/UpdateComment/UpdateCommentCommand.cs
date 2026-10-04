using Application.Models.Comments;
using MediatR;

namespace Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentRequest(string Content);

public record UpdateCommentCommand(long Id, string Content) : IRequest<CommentsItemModal>;
