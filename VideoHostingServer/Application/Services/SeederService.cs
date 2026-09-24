using System.Text.Json;
using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Video;
using Domain;
using Domain.Entities.Video;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class SeederService(
    AppDbContext appDbContext,
    VideoMappingProfile videoMapper,
    IImageService imageService,
    IVideoFileService videoFileService
) : ISeederService
{
    public async Task SeedVideoPrivaciesAsync()
    {
        if (await appDbContext.VideoPrivacies.AnyAsync())
            return;

        var privacies = new List<VideoPrivacyEntity>
        {
            new() { Name = "Публічне", SystemCode = VideoPrivacyConstants.Public },
            new() { Name = "Приватне", SystemCode = VideoPrivacyConstants.Private },
            new() { Name = "За посиланням", SystemCode = VideoPrivacyConstants.UrlOnly },
        };

        await appDbContext.VideoPrivacies.AddRangeAsync(privacies);
        await appDbContext.SaveChangesAsync();
    }

    public async Task SeedVideosAsync(string jsonPath, string videosFolder)
    {
        if (!File.Exists(jsonPath))
            return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var videosData = JsonSerializer.Deserialize<List<VideoSeedModel>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (videosData != null)
        {
            var privacies = await appDbContext.VideoPrivacies.ToListAsync();
            var publicPrivacy = privacies.FirstOrDefault(p =>
                p.SystemCode == VideoPrivacyConstants.Public
            );

            foreach (var v in videosData)
            {
                if (await appDbContext.Videos.AnyAsync(vid => vid.Slug == v.Slug))
                    continue;

                var entity = videoMapper.MapToEntity(v);

                var privacy =
                    privacies.FirstOrDefault(p => p.SystemCode == v.PrivacySystemCode)
                    ?? publicPrivacy;
                if (privacy != null)
                {
                    entity.PrivacyId = privacy.Id;
                }

                if (!string.IsNullOrEmpty(v.ImagePath))
                    entity.Image = await imageService.SaveImageFromUrlAsync(v.ImagePath);

                if (!string.IsNullOrEmpty(v.VideoFile))
                {
                    var videoPath = Path.Combine(videosFolder, v.VideoFile);
                    if (File.Exists(videoPath))
                    {
                        entity.Video = await videoFileService.SaveVideoFromFilePathAsync(videoPath);
                    }
                }

                await appDbContext.Videos.AddAsync(entity);
                await appDbContext.SaveChangesAsync();
            }
        }
    }

    public async Task UpdateDatabase()
    {
        await appDbContext.Database.MigrateAsync();
    }
}
