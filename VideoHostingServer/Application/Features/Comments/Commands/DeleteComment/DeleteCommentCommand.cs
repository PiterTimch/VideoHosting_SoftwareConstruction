using MediatR;

namespace Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(long Id) : IRequest<Unit>;
