using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public class RoleSeeder(RoleManager<RoleEntity> roleManager) : IRoleSeeder
{
    public async Task SeedAsync()
    {
        foreach (var roleName in Roles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new RoleEntity { Name = roleName });
                if (!result.Succeeded)
                    Console.WriteLine($"Error Create Role {roleName}");
            }
        }
    }
}
