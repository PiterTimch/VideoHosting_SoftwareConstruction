using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Accounts.Commands.Register;

public class RegisterHandler(UserManager<UserEntity> userManager,
    IJwtTokenService jwtTokenService,
    UserMapping mapper,
    IImageService imageService,
    ICookieAuthService cookieAuthService) : IRequestHandler<RegisterCommand, string>
{
    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var checkUser = await userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Model.Email, cancellationToken);

        if (checkUser != null)
        {
            if (checkUser.IsDeleted)
                throw new Exception("Цей користувач видалений. Будь ласка, зверніться в підтримку");
            throw new Exception("За цією поштою уже зареєстрований користувач");
        }

        if (request.Model.Password.Length < 6)
            throw new Exception("Мінімальна довжина паролю 6 символів");

        var user = mapper.MapToEntity(request.Model);

        user.Image = await imageService.SaveImageAsync(request.Model.ImageFile);

        var result = await userManager.CreateAsync(user, request.Model.Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, Roles.User);
            var token = await jwtTokenService.CreateTokenAsync(user);
            cookieAuthService.SetAuthCookie(token);
            return token;
        }
        throw new Exception("Помилка реєстрації");
    }
}
