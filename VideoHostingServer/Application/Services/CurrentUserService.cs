using Application.Constants;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public long GetCurrentUserId()
    {
        var userId = TryGetCurrentUserId();

        if (userId is null)
            throw new UnFreelancerizedAccessException(
                "Користувач не пройшов автентифікацію або відсутня заявка NameIdentifier.");

        return userId.Value;
    }

    public long? TryGetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (claim is null)
            return null;

        return long.TryParse(claim, out var id) ? id : null;
    }

    public bool IsInRole(string role)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
            return false;

        if (user.IsInRole(role))
            return true;

        foreach (var claim in user.FindAll(ClaimTypes.Role))
        {
            if (string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var rolesClaim = user.FindFirstValue(AuthConstants.RolesClaim);
        if (string.IsNullOrWhiteSpace(rolesClaim))
            return false;

        if (rolesClaim.StartsWith('['))
        {
            var roles = JsonSerializer.Deserialize<List<string>>(rolesClaim) ?? [];
            return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        return string.Equals(rolesClaim, role, StringComparison.OrdinalIgnoreCase);
    }
}
