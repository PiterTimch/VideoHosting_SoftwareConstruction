using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Video;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Videos.Queries.GetVideoStream;

public class GetVideoStreamQueryHandler(
    IGenericRepository<VideoEntity, long> repo,
    ICurrentUserService currentUser,
    IVideoAccessTokenService tokenService,
    IConfiguration configuration)
    : IRequestHandler<GetVideoStreamQuery, IActionResult>
{
    public async Task<IActionResult> Handle(
        GetVideoStreamQuery request,
        CancellationToken cancellationToken)
    {
        var baseFileName = ExtractBaseFileName(request.VideoName);

        var video = await repo.AsQurable()
            .Include(v => v.Privacy)
            .Include(v => v.Portfolio).ThenInclude(p => p!.Freelancer)
            .Where(v => (v.Video == baseFileName || v.Video == request.VideoName) && !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (video == null)
            return new NotFoundResult();

        var privacyCode = video.Privacy?.SystemCode ?? VideoPrivacyConstants.Public;

        if (privacyCode != VideoPrivacyConstants.Public)
        {
            var tokenValid = !string.IsNullOrEmpty(request.Token)
                && tokenService.ValidateToken(request.Token, video.Id);

            var userId = currentUser.TryGetCurrentUserId();
            var isAdmin = currentUser.IsInRole(Roles.Admin);
            var isOwner = video.Portfolio?.Freelancer?.Id == userId;
            if (!tokenValid && !isAdmin && !isOwner)
                return new ForbidResult();
        }

        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot)
            ? Directory.GetCurrentDirectory()
            : mediaRoot;
        var videosDir = configuration["VideosDir"]!;
        var safeVideoName = Path.GetFileName(request.VideoName);
        var filePath = Path.Combine(basePath, videosDir, safeVideoName);

        if (!File.Exists(filePath))
            return new NotFoundResult();

        return BuildStreamResult(filePath, request.RangeHeader);
    }

    private static string ExtractBaseFileName(string videoName)
    {
        var parts = videoName.Split('_');
        return parts.Length > 1 ? string.Join("_", parts.Skip(1)) : videoName;
    }

    private static IActionResult BuildStreamResult(string filePath, string? rangeHeader)
    {
        var fileInfo = new FileInfo(filePath);
        var totalLength = fileInfo.Length;
        const string contentType = "video/mp4";

        if (string.IsNullOrEmpty(rangeHeader))
        {
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, true);
            return new FileStreamResult(stream, contentType)
            {
                EnableRangeProcessing = true,
            };
        }

        var range = ParseRange(rangeHeader, totalLength);
        if (range == null)
            return new StatusCodeResult(416);

        var (start, end) = range.Value;
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, true);
        fileStream.Seek(start, SeekOrigin.Begin);

        return new RangeFileStreamResult(fileStream, contentType, start, end, totalLength);
    }

    private static (long start, long end)? ParseRange(string rangeHeader, long totalLength)
    {
        if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
            return null;

        var rangeValue = rangeHeader["bytes=".Length..];
        var parts = rangeValue.Split('-');
        if (parts.Length != 2) return null;

        if (totalLength <= 0) return null;
        if (string.IsNullOrEmpty(parts[0]))
        {
            if (!long.TryParse(parts[1], out var suffixLength) || suffixLength <= 0) return null;
            return (Math.Max(0, totalLength - suffixLength), totalLength - 1);
        }

        if (!long.TryParse(parts[0], out var start)) return null;
        long end;
        if (string.IsNullOrEmpty(parts[1])) end = totalLength - 1;
        else if (!long.TryParse(parts[1], out end)) return null;

        if (start < 0 || start > end || start >= totalLength) return null;
        end = Math.Min(end, totalLength - 1);
        return (start, end);
    }
}
