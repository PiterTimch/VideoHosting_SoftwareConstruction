using Application.Interfaces;
using Domain.Entities.Video;

namespace Application.Constants;

public static class VideoAccessQueries
{
    public static IQueryable<VideoEntity> ForCurrentUser(
        this IQueryable<VideoEntity> query,
        ICurrentUserService currentUser)
    {
        if (currentUser.IsInRole(Roles.Admin))
            return query;

        if (currentUser.TryGetCurrentUserId() is long userId)
        {
            return query.Where(v =>
                v.ChannelId == userId ||
                v.Privacy!.SystemCode == VideoPrivacyConstants.Public);
        }

        return query.Where(v => v.Privacy!.SystemCode == VideoPrivacyConstants.Public);
    }

    public static IQueryable<VideoEntity> ForDirectAccess(
        this IQueryable<VideoEntity> query,
        ICurrentUserService currentUser)
    {
        if (currentUser.IsInRole(Roles.Admin))
            return query;

        if (currentUser.TryGetCurrentUserId() is long userId)
        {
            return query.Where(v =>
                v.ChannelId == userId ||
                v.Privacy!.SystemCode == VideoPrivacyConstants.Public ||
                v.Privacy!.SystemCode == VideoPrivacyConstants.UrlOnly);
        }

        return query.Where(v =>
            v.Privacy!.SystemCode == VideoPrivacyConstants.Public ||
            v.Privacy!.SystemCode == VideoPrivacyConstants.UrlOnly);
    }
}
