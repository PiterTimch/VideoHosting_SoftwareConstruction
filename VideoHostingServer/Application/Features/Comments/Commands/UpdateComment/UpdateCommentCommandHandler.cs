using Application.Interfaces;
using Application.Mappings;
using Application.Models.Comments;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler(
    AppDbContext context,
    CommentMappingProfile mapper,
    ICurrentUserService currentUserService
) : IRequestHandler<UpdateCommentCommand, CommentsItemModal>
{
    public async Task<CommentsItemModal> Handle(
        UpdateCommentCommand request,
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
            throw new UnauthorizedAccessException("Ви не можете редагувати чужий коментар");

        comment.Content = request.Content;
        comment.IsEdited = true;

        await context.SaveChangesAsync(cancellationToken);

        return await mapper.ProjectToItemModel(
            context.Comments.AsNoTracking())
            .FirstAsync(x => x.Id == comment.Id, cancellationToken);
    }
}
