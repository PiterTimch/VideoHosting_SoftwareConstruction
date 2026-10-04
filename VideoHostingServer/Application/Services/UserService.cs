using Application.Interfaces;
using Application.Models.User;
using Domain;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(AppDbContext context, UserManager<UserEntity> userManager) : IUserService
{
    public async Task LoadLoginsAndRolesAsync(List<UserItemModel> users)
    {
        await context.UserLogins.ForEachAsync(login =>
        {
            var user = users.FirstOrDefault(u => u.Id == login.UserId);
            if (user != null)
            {
                user.LoginTypes.Add(login.LoginProvider);
            }
        });

        var identityUsers = await userManager.Users.AsNoTracking().ToListAsync();

        foreach (var identityUser in identityUsers)
        {
            var adminUser = users.FirstOrDefault(u => u.Id == identityUser.Id);
            if (adminUser != null)
            {
                var roles = await userManager.GetRolesAsync(identityUser);
                adminUser.Roles = roles.ToList();

                if (!string.IsNullOrEmpty(identityUser.PasswordHash))
                {
                    adminUser.LoginTypes.Add("Password");
                }
            }
        }
    }
}
