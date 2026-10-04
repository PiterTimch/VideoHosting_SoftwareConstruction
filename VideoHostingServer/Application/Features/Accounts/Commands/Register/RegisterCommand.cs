using Application.Models.Account;
using MediatR;

namespace Application.Features.Accounts.Commands.Register;

public class RegisterCommand(AccountRegisterModel model) : IRequest<string>
{
    public AccountRegisterModel Model { get; } = model;
}
