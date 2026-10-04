using System.Text.Json;
using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.User;
using Application.Models.Video;
using Domain;
using Domain.Entities.Channel;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class SeederService(
    AppDbContext appDbContext,
    RoleManager<RoleEntity> roleManager,
    UserManager<UserEntity> userManager,
    VideoMappingProfile videoMapper,
    UserMapping userMapper,
    IImageService imageService,
    IVideoFileService videoFileService
) : ISeederService
{
    public async Task SeedRolesAsync()
    {
        foreach (var roleName in Roles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new RoleEntity { Name = roleName });
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Error Create Role {roleName}");
                }
            }
        }
    }

    public async Task SeedUsersAsync(string jsonPath)
    {
        if (await appDbContext.Users.AnyAsync())
            return;

        if (!File.Exists(jsonPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(jsonPath);

            var users = JsonSerializer.Deserialize<List<UserSeedModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (users == null || users.Count == 0)
                return;

            foreach (var user in users)
            {
                var entity = userMapper.MapToEntity(user);
                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    entity.Image = await imageService.SaveImageFromUrlAsync(user.ImagePath);
                }

                var result = await userManager.CreateAsync(entity, user.Password);
                if (!result.Succeeded)
                {
                    Console.WriteLine("Error Create User {0}", user.Email);
                    continue;
                }

                var channel = new ChannelEntity
                {
                    Id = entity.Id,
                    Name = $"{entity.FirstName} {entity.LastName}".Trim(),
                    NickName = entity.UserName ?? entity.Email?.Split('@')[0] ?? $"user_{entity.Id}",
                    Freelancer = entity,
                };
                await appDbContext.Channels.AddAsync(channel);
                await appDbContext.SaveChangesAsync();

                foreach (var role in user.Roles)
                {
                    if (await roleManager.RoleExistsAsync(role))
                    {
                        await userManager.AddToRoleAsync(entity, role);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seed users error: {ex.Message}");
        }
    }

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

            var channel = await appDbContext.Channels.FirstOrDefaultAsync();

            foreach (var v in videosData)
            {
                if (await appDbContext.Videos.AnyAsync(vid => vid.Slug == v.Slug))
                    continue;

                var entity = videoMapper.MapToEntity(v);

                if (channel != null)
                {
                    entity.ChannelId = channel.Id;
                }

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
