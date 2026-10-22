using Application.Constants;
using Application.Interfaces;
using Application.Exceptions;
using Application.Mappings;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Videos.Queries.GetByVideo;

public class GetByVideoQueryHandler(
    IGenericRepository<VideoEntity, long> repo,
    VideoMappingProfile mapper,
    ICurrentUserService currentUser,
    IVideoAccessTokenService tokenService)
    : IRequestHandler<GetByVideoQuery, VideoItemModel>
{
    public async Task<VideoItemModel> Handle(GetByVideoQuery request, CancellationToken cancellationToken)
    {
        IQueryable<VideoEntity> query = repo.AsQurable()
            .Include(x => x.Privacy)
            .Include(x => x.Portfolio).ThenInclude(p => p!.Freelancer)
            .Where(x => !x.IsDeleted);
        query = string.IsNullOrWhiteSpace(request.Token)
            ? query.ForDirectAccess(currentUser)
            : query;

        if (request.Model.Id != null)
        {
            query = query.Where(x => x.Id == request.Model.Id.Value);
        }
        else if (!string.IsNullOrEmpty(request.Model.Slug))
        {
            query = query.Where(x => x.Slug == request.Model.Slug);
        }
        else
        {
            throw new Exception("Необхідно вказати Id або Slug");
        }

        var video = await query
            .FirstOrDefaultAsync(cancellationToken);

        if (video == null)
            throw new Exception("Відео не знайдено");

        var isOwner = video.Portfolio?.Freelancer?.Id == currentUser.TryGetCurrentUserId();
        var isAdmin = currentUser.IsInRole(Roles.Admin);
        var privacy = video.Privacy?.SystemCode;
        var tokenValid = !string.IsNullOrWhiteSpace(request.Token)
            && tokenService.ValidateToken(request.Token, video.Id);

        if (privacy == VideoPrivacyConstants.Private && !isOwner && !isAdmin && !tokenValid)
            throw new UnFreelancerizedAccessException("Доступ заборонено");

        if (privacy == VideoPrivacyConstants.UrlOnly && !isOwner && !isAdmin && !tokenValid)
            throw new UnFreelancerizedAccessException("Доступ заборонено");

        return await mapper.ProjectToItemModel(repo.AsQurable().Where(x => x.Id == video.Id))
            .FirstAsync(cancellationToken);
    }
}
