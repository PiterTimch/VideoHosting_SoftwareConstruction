using Application.Exceptions;
using Application.Features.Accounts.Commands.Login;
using Application.Features.Accounts.Commands.Logout;
using Application.Features.Accounts.Commands.RefreshToken;
using Application.Features.Accounts.Commands.Register;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.Account;
using Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace UnitTests.Handlers;

public class AccountHandlersTests
{
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ICookieAuthService> _cookieAuthServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IImageService> _imageServiceMock;
    private readonly UserMapping _userMapper = new();

    public AccountHandlersTests()
    {
        var store = new Mock<IUserStore<UserEntity>>();
        _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _cookieAuthServiceMock = new Mock<ICookieAuthService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _imageServiceMock = new Mock<IImageService>();
    }

    [Fact]
    public async Task LoginHandler_ValidUser_ReturnsTokenAndSetsCookie()
    {
        var handler = new LoginHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object);
        var user = new UserEntity { Email = "user@test.com", IsDeleted = false };
        var users = new List<UserEntity> { user }.BuildMockDbSet();

        _userManagerMock.Setup(u => u.Users).Returns(users.Object);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);
        _jwtTokenServiceMock.Setup(j => j.CreateTokenAsync(user)).ReturnsAsync("valid_jwt");

        var token = await handler.Handle(new LoginCommand(new AccountLoginModel { Email = "user@test.com", Password = "Pass123!" }), default);

        token.Should().Be("valid_jwt");
        _cookieAuthServiceMock.Verify(c => c.SetAuthCookie("valid_jwt"), Times.Once);
    }

    [Fact]
    public async Task LoginHandler_InvalidPasswordOrNotFound_ThrowsException()
    {
        var handler = new LoginHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object);
        var users = new List<UserEntity>().BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);

        await FluentActions.Invoking(() => handler.Handle(new LoginCommand(new AccountLoginModel { Email = "user@test.com", Password = "Wrong" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Неправильний логін або пароль");
    }

    [Fact]
    public async Task LoginHandler_DeletedUser_ThrowsException()
    {
        var handler = new LoginHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object);
        var user = new UserEntity { Email = "user@test.com", IsDeleted = true };
        var users = new List<UserEntity> { user }.BuildMockDbSet();

        _userManagerMock.Setup(u => u.Users).Returns(users.Object);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);

        await FluentActions.Invoking(() => handler.Handle(new LoginCommand(new AccountLoginModel { Email = "user@test.com", Password = "Pass123!" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Цей користувач видалений. Будь ласка, зверніться в підтримку");
    }

    [Fact]
    public async Task RegisterHandler_ExistingUser_ThrowsException()
    {
        var handler = new RegisterHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _userMapper, _imageServiceMock.Object, _cookieAuthServiceMock.Object);
        var existingUser = new UserEntity { Email = "existing@test.com", IsDeleted = false };
        var users = new List<UserEntity> { existingUser }.BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);

        await FluentActions.Invoking(() => handler.Handle(new RegisterCommand(new AccountRegisterModel { Email = "existing@test.com", Password = "Password123!" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("За цією поштою уже зареєстрований користувач");
    }

    [Fact]
    public async Task RegisterHandler_DeletedExistingUser_ThrowsException()
    {
        var handler = new RegisterHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _userMapper, _imageServiceMock.Object, _cookieAuthServiceMock.Object);
        var deletedUser = new UserEntity { Email = "deleted@test.com", IsDeleted = true };
        var users = new List<UserEntity> { deletedUser }.BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);

        await FluentActions.Invoking(() => handler.Handle(new RegisterCommand(new AccountRegisterModel { Email = "deleted@test.com", Password = "Password123!" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Цей користувач видалений. Будь ласка, зверніться в підтримку");
    }

    [Fact]
    public async Task RegisterHandler_ShortPassword_ThrowsException()
    {
        var handler = new RegisterHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _userMapper, _imageServiceMock.Object, _cookieAuthServiceMock.Object);
        var users = new List<UserEntity>().BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);

        await FluentActions.Invoking(() => handler.Handle(new RegisterCommand(new AccountRegisterModel { Email = "new@test.com", Password = "123" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Мінімальна довжина паролю 6 символів");
    }

    [Fact]
    public async Task RegisterHandler_Success_ReturnsTokenAndSetsCookie()
    {
        var handler = new RegisterHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _userMapper, _imageServiceMock.Object, _cookieAuthServiceMock.Object);
        var users = new List<UserEntity>().BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<UserEntity>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        _jwtTokenServiceMock.Setup(j => j.CreateTokenAsync(It.IsAny<UserEntity>())).ReturnsAsync("new_jwt");

        var token = await handler.Handle(new RegisterCommand(new AccountRegisterModel { Email = "new@test.com", Password = "Password123!" }), default);

        token.Should().Be("new_jwt");
        _cookieAuthServiceMock.Verify(c => c.SetAuthCookie("new_jwt"), Times.Once);
    }

    [Fact]
    public async Task RegisterHandler_CreateFailed_ThrowsException()
    {
        var handler = new RegisterHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _userMapper, _imageServiceMock.Object, _cookieAuthServiceMock.Object);
        var users = new List<UserEntity>().BuildMockDbSet();
        _userManagerMock.Setup(u => u.Users).Returns(users.Object);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());

        await FluentActions.Invoking(() => handler.Handle(new RegisterCommand(new AccountRegisterModel { Email = "new@test.com", Password = "Password123!" }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Помилка реєстрації");
    }

    [Fact]
    public async Task LogoutHandler_ClearsCookie()
    {
        var handler = new LogoutHandler(_cookieAuthServiceMock.Object);
        await handler.Handle(new LogoutCommand(), default);

        _cookieAuthServiceMock.Verify(c => c.ClearAuthCookie(), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenHandler_ValidUser_ReturnsToken()
    {
        var handler = new RefreshTokenHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object, _currentUserServiceMock.Object);
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(10);
        var user = new UserEntity { Id = 10, IsDeleted = false };
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(j => j.CreateTokenAsync(user)).ReturnsAsync("refreshed_jwt");

        var token = await handler.Handle(new RefreshTokenCommand(), default);

        token.Should().Be("refreshed_jwt");
        _cookieAuthServiceMock.Verify(c => c.SetAuthCookie("refreshed_jwt"), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenHandler_UserNotFoundOrDeleted_ThrowsException()
    {
        var handler = new RefreshTokenHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object, _currentUserServiceMock.Object);
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(10);
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync((UserEntity?)null);

        await FluentActions.Invoking(() => handler.Handle(new RefreshTokenCommand(), default))
            .Should().ThrowAsync<UnFreelancerizedAccessException>().WithMessage("Користувача не знайдено");
    }
}
