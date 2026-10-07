using Application.Features.Accounts.Commands.ChangePassword;
using Application.Features.Accounts.Commands.Login;
using Application.Features.Accounts.Commands.Logout;
using Application.Features.Accounts.Commands.Register;
using Application.Features.Accounts.Commands.RefreshToken;
using Application.Models.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VideoHostingServer.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AccountLoginModel model)
    {
        try
        {
            var command = new LoginCommand(model);
            var token = await mediator.Send(command);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Register([FromForm] AccountRegisterModel model)
    {
        try
        {
            var command = new RegisterCommand(model);
            var token = await mediator.Send(command);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] AccountChangePasswordModel model)
    {
        var command = new ChangePasswordCommand(model);
        await mediator.Send(command);

        return Ok();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var command = new RefreshTokenCommand();
            var token = await mediator.Send(command);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await mediator.Send(new LogoutCommand());
        return Ok();
    }
}
