using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Accounts.Commands.RefreshToken;

public class RefreshTokenHandler(
    UserManager<UserEntity> userManager,
    IJwtTokenService jwtTokenService,
    ICookieAuthService cookieAuthService,
    ICurrentUserService currentUserService
) : IRequestHandler<RefreshTokenCommand, string>
{
    public async Task<string> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
            throw new UnFreelancerizedAccessException("Користувача не знайдено");

        var token = await jwtTokenService.CreateTokenAsync(user);
        cookieAuthService.SetAuthCookie(token);
        return token;
    }
}
