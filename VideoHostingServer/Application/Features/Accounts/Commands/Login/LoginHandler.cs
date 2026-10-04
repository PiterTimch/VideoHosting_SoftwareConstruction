using Application.Interfaces;
using Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.Login;

public class LoginHandler(UserManager<UserEntity> userManager,
    IJwtTokenService jwtTokenService,
    ICookieAuthService cookieAuthService) : IRequestHandler<LoginCommand, string>
{
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Model.Email, cancellationToken);

        if (user != null && await userManager.CheckPasswordAsync(user, request.Model.Password))
        {
            if (user.IsDeleted)
            {
                throw new Exception("Цей користувач видалений. Будь ласка, зверніться в підтримку");
            }

            var token = await jwtTokenService.CreateTokenAsync(user);
            cookieAuthService.SetAuthCookie(token);
            return token;
        }

        throw new Exception("Неправильний логін або пароль");
    }
}
