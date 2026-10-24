using System.Text.Json;
using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class VideoSeeder(
    AppDbContext appDbContext,
    VideoMappingProfile videoMapper,
    IImageService imageService,
    IVideoFileService videoFileService) : IVideoSeeder
{
    public async Task SeedAsync(string jsonPath, string videosFolder)
    {
        if (!File.Exists(jsonPath))
            return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var videosData = JsonSerializer.Deserialize<List<VideoSeedModel>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (videosData == null)
            return;

        var privacies = await appDbContext.VideoPrivacies.ToListAsync();
        var publicPrivacy = privacies.FirstOrDefault(p => p.SystemCode == VideoPrivacyConstants.Public);
        var portfolio = await appDbContext.Portfolios.FirstOrDefaultAsync();

        foreach (var video in videosData)
        {
            if (await appDbContext.Videos.AnyAsync(item => item.Slug == video.Slug))
                continue;

            var entity = videoMapper.MapToEntity(video);
            if (portfolio != null)
                entity.PortfolioId = portfolio.Id;

            var privacy = privacies.FirstOrDefault(item => item.SystemCode == video.PrivacySystemCode) ?? publicPrivacy;
            if (privacy != null)
                entity.PrivacyId = privacy.Id;

            if (!string.IsNullOrEmpty(video.ImagePath))
            {
                try
                {
                    entity.Image = await imageService.SaveImageFromUrlAsync(video.ImagePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading video image from URL {video.ImagePath}: {ex.Message}");
                }
            }

            if (!string.IsNullOrEmpty(video.VideoFile))
            {
                var videoPath = Path.Combine(videosFolder, video.VideoFile);
                if (File.Exists(videoPath))
                    entity.Video = await videoFileService.SaveVideoFromFilePathAsync(videoPath);
            }

            await appDbContext.Videos.AddAsync(entity);
            await appDbContext.SaveChangesAsync();
        }
    }
}
