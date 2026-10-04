using Application.Models.Account;
using MediatR;

namespace Application.Features.Accounts.Commands.Login;

public class LoginCommand(AccountLoginModel model) : IRequest<string>
{
    public AccountLoginModel Model { get; } = model;
}
