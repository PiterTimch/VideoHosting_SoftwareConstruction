using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Comments;
using Domain;
using Domain.Entities.Comments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Comments.Commands.CreateComments;

public class CreateCommentCommandHandler(
    AppDbContext context,
    CommentMappingProfile mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCommentCommand, CommentsItemModal>
{
    public async Task<CommentsItemModal> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        var videoExists = await context.Videos
            .Where(x => x.Id == request.VideoId && !x.IsDeleted)
            .ForDirectAccess(currentUser)
            .AnyAsync(cancellationToken);

        if (!videoExists)
            throw new Exception("Відео не знайдено");

        var comment = new CommentsEntity
        {
            Content = request.Content,
            VideoId = request.VideoId,
            ParentId = request.ParentId,
            UserId = currentUser.GetCurrentUserId(),
        };

        if (request.ParentId.HasValue)
        {
            var parent = await context.Comments
                .Include(x => x.Parent)
                    .ThenInclude(p => p!.Parent)
                .FirstOrDefaultAsync(
                    x => x.Id == request.ParentId.Value,
                    cancellationToken
                );

            if (parent == null)
            {
                throw new Exception("Батьківський коментар не знайдено");
            }

            int parentDepth = 0;
            var current = parent;
            while (current.Parent != null)
            {
                parentDepth++;
                current = current.Parent;
            }

            if (parentDepth >= 2)
            {
                throw new Exception("Досягнуто максимальну глибину відповідей (максимум 2 рівні)");
            }

            parent.RepliesCount++;
        }

        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return await mapper.ProjectToItemModel(
            context.Comments.AsNoTracking()
                .Where(x => x.Id == comment.Id))
            .FirstAsync(cancellationToken);
    }
}
