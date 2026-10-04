using Application.Interfaces;
using MediatR;

namespace Application.Features.Accounts.Commands.Logout;

public class LogoutHandler(ICookieAuthService cookieAuthService) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        cookieAuthService.ClearAuthCookie();
        return Task.CompletedTask;
    }
}
