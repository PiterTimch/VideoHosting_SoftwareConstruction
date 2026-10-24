using Application.Constants;
using Application.Interfaces;
using Domain;
using Domain.Entities.Video;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class VideoPrivacySeeder(AppDbContext appDbContext) : IVideoPrivacySeeder
{
    public async Task SeedAsync()
    {
        if (await appDbContext.VideoPrivacies.AnyAsync())
            return;

        await appDbContext.VideoPrivacies.AddRangeAsync(
            new VideoPrivacyEntity { Name = "Публічне", SystemCode = VideoPrivacyConstants.Public },
            new VideoPrivacyEntity { Name = "Приватне", SystemCode = VideoPrivacyConstants.Private },
            new VideoPrivacyEntity { Name = "За посиланням", SystemCode = VideoPrivacyConstants.UrlOnly });
        await appDbContext.SaveChangesAsync();
    }
}
