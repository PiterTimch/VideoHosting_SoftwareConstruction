using Application.Constants;
using Application.Exceptions;
using Application.Interfaces;
using Application.Models.Video;
using Domain.Entities.Video;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Videos.Commands.GenerateVideoAccessToken;

public class GenerateVideoAccessTokenHandler(
    IGenericRepository<VideoEntity, long> repo,
    ICurrentUserService currentUser,
    IVideoAccessTokenService tokenService,
    IConfiguration configuration)
    : IRequestHandler<GenerateVideoAccessTokenCommand, VideoAccessTokenResponse>
{
    public async Task<VideoAccessTokenResponse> Handle(
        GenerateVideoAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var video = await repo.AsQurable()
            .Include(v => v.Privacy)
            .Include(v => v.Portfolio).ThenInclude(p => p!.Freelancer)
            .Where(v => v.Id == request.VideoId && !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Exception("Відео не знайдено");

        var userId = currentUser.TryGetCurrentUserId();
        var isAdmin = currentUser.IsInRole(Roles.Admin);
        var isOwner = video.Portfolio?.Freelancer?.Id == userId;

        if (!isAdmin && !isOwner)
            throw new UnFreelancerizedAccessException("Доступ заборонено");

        var expiryMinutes = configuration.GetValue("VideoAccessToken:ExpiryMinutes", 60);
        var expiresIn = TimeSpan.FromMinutes(expiryMinutes);
        var token = tokenService.GenerateToken(request.VideoId, expiresIn);
        var expiresAt = DateTime.UtcNow.Add(expiresIn);

        return new VideoAccessTokenResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
        };
    }
}
