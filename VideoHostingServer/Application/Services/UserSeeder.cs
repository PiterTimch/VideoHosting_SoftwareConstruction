using System.Text.Json;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.User;
using Domain;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserSeeder(
    AppDbContext appDbContext,
    RoleManager<RoleEntity> roleManager,
    UserManager<UserEntity> userManager,
    UserMapping userMapper,
    IImageService imageService) : IUserSeeder
{
    public async Task SeedAsync(string jsonPath)
    {
        if (await appDbContext.Users.AnyAsync() || !File.Exists(jsonPath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var users = JsonSerializer.Deserialize<List<UserSeedModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (users == null || users.Count == 0)
                return;

            foreach (var user in users)
            {
                var entity = userMapper.MapToEntity(user);
                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    try
                    {
                        entity.Image = await imageService.SaveImageFromUrlAsync(user.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading user image from URL {user.ImagePath}: {ex.Message}");
                    }
                }

                var result = await userManager.CreateAsync(entity, user.Password);
                if (!result.Succeeded)
                {
                    Console.WriteLine("Error Create User {0}", user.Email);
                    continue;
                }

                await appDbContext.Portfolios.AddAsync(new PortfolioEntity
                {
                    Id = entity.Id,
                    Name = $"{entity.FirstName} {entity.LastName}".Trim(),
                    NickName = entity.UserName ?? entity.Email?.Split('@')[0] ?? $"user_{entity.Id}",
                    Freelancer = entity,
                });
                await appDbContext.SaveChangesAsync();

                foreach (var role in user.Roles)
                {
                    if (await roleManager.RoleExistsAsync(role))
                        await userManager.AddToRoleAsync(entity, role);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seed users error: {ex.Message}");
        }
    }
}
