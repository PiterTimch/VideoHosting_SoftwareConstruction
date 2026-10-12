using Application.Features.Accounts.Commands.ChangePassword;
using Application.Features.Accounts.Commands.Login;
using Application.Features.Accounts.Commands.Logout;
using Application.Features.Accounts.Commands.RefreshToken;
using Application.Features.Accounts.Commands.Register;
using Application.Models.Account;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoHostingServer.Controllers;
using Xunit;

namespace UnitTests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AccountController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var model = new AccountLoginModel { Email = "user@test.com", Password = "Password123!" };
        _mediatorMock.Setup(m => m.Send(It.Is<LoginCommand>(c => c.Model == model), default))
            .ReturnsAsync("sample_jwt_token");

        var result = await _controller.Login(model);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequestWithMessage()
    {
        var model = new AccountLoginModel { Email = "user@test.com", Password = "WrongPassword" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
            .ThrowsAsync(new Exception("Неправильний логін або пароль"));

        var result = await _controller.Login(model);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ValidModel_ReturnsOkWithToken()
    {
        var model = new AccountRegisterModel { Email = "new@test.com", Password = "Password123!" };
        _mediatorMock.Setup(m => m.Send(It.Is<RegisterCommand>(c => c.Model == model), default))
            .ReturnsAsync("sample_jwt_token");

        var result = await _controller.Register(model);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ExistingEmail_ReturnsBadRequestWithMessage()
    {
        var model = new AccountRegisterModel { Email = "existing@test.com", Password = "Password123!" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), default))
            .ThrowsAsync(new Exception("За цією поштою уже зареєстрований користувач"));

        var result = await _controller.Register(model);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangePassword_ValidModel_ReturnsOk()
    {
        var model = new AccountChangePasswordModel { OldPassword = "old", NewPassword = "new" };

        var result = await _controller.ChangePassword(model);

        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<ChangePasswordCommand>(c => c.Model == model), default), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_ValidUser_ReturnsOkWithToken()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ReturnsAsync("refreshed_jwt_token");

        var result = await _controller.RefreshToken();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_Unauthorized_ReturnsBadRequest()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ThrowsAsync(new Exception("Користувача не знайдено"));

        var result = await _controller.RefreshToken();

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Logout_CallsLogoutCommand_ReturnsOk()
    {
        var result = await _controller.Logout();

        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<LogoutCommand>(), default), Times.Once);
    }
}
