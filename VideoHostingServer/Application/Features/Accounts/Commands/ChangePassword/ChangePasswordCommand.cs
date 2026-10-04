using Application.Models.Account;
using MediatR;

namespace Application.Features.Accounts.Commands.ChangePassword;

public class ChangePasswordCommand(AccountChangePasswordModel model) : IRequest
{
    public AccountChangePasswordModel Model { get; } = model;
}
