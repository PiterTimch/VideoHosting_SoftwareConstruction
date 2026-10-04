using Application.Constants;
using Application.Interfaces;
using Domain.Entities.Identity;
using Domain.Entities.Channel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Application.Services;

public class JwtTokenService(
    IConfiguration configuration,
    UserManager<UserEntity> userManager,
    IGenericRepository<ChannelEntity, long> channelRepo) : IJwtTokenService
{
    public async Task<string> CreateTokenAsync(UserEntity user)
    {
        var key = configuration["Jwt:Key"];

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("name", $"{user.LastName} {user.FirstName}"),
            new Claim("image", $"{user.Image}")
        };

        var channel = await channelRepo.AsQurable()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Freelancer != null && c.Freelancer.Id == user.Id);

        if (channel != null)
        {
            claims.Add(new Claim("channelId", channel.Id.ToString()));
        }

        var roles = await userManager.GetRolesAsync(user);
        var rolesJson = JsonSerializer.Serialize(roles);

        claims.Add(new Claim(AuthConstants.RolesClaim, rolesJson, JsonClaimValueTypes.JsonArray));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key ?? "SecretKeyForVideoHostingProject2026LabWorks!!");

        var symmetricSecurityKey = new SymmetricSecurityKey(keyBytes);

        var signingCredentials = new SigningCredentials(
            symmetricSecurityKey,
            SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: signingCredentials);

        string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return token;
    }
}
