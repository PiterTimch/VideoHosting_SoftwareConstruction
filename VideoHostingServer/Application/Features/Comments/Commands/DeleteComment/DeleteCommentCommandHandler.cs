using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler(
    AppDbContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<DeleteCommentCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var comment = await context.Comments.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken
        );

        if (comment == null)
            throw new Exception("Коментар не знайдено");

        var currentUserId = currentUserService.GetCurrentUserId();
        if (comment.UserId != currentUserId)
            throw new UnauthorizedAccessException("Ви не можете видалити чужий коментар");

        await context.Comments
            .Where(x => x.ParentId == comment.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (comment.ParentId.HasValue)
        {
            await context.Comments
                .Where(x => x.Id == comment.ParentId.Value)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(c => c.RepliesCount, c => c.RepliesCount - 1),
                    cancellationToken
                );
        }

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
