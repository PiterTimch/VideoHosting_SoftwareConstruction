using Application.Features.Accounts.Commands.Login;
using Application.Interfaces;
using Application.Models.Account;
using Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    public class AuthHandlerTests
    {
        private readonly Mock<UserManager<UserEntity>> _userManagerMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<ICookieAuthService> _cookieAuthServiceMock;
        private readonly LoginHandler _handler;

        public AuthHandlerTests()
        {
            var store = new Mock<IUserStore<UserEntity>>();
            _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _cookieAuthServiceMock = new Mock<ICookieAuthService>();

            _handler = new LoginHandler(_userManagerMock.Object, _jwtTokenServiceMock.Object, _cookieAuthServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsToken()
        {
            var command = new LoginCommand(new AccountLoginModel { Email = "test@test.com", Password = "Password123!" });
            var user = new UserEntity { Email = "test@test.com", IsDeleted = false };
            
            var usersList = new List<UserEntity> { user }.AsQueryable();
            _userManagerMock.Setup(x => x.Users).Returns(usersList);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);
            _jwtTokenServiceMock.Setup(x => x.CreateTokenAsync(user)).ReturnsAsync("valid_token");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be("valid_token");
            _cookieAuthServiceMock.Verify(x => x.SetAuthCookie("valid_token"), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidCredentials_ThrowsException()
        {
            var command = new LoginCommand(new AccountLoginModel { Email = "test@test.com", Password = "WrongPassword" });
            var user = new UserEntity { Email = "test@test.com", IsDeleted = false };
            
            var usersList = new List<UserEntity> { user }.AsQueryable();
            _userManagerMock.Setup(x => x.Users).Returns(usersList);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

            await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
                .Should().ThrowAsync<System.Exception>().WithMessage("Неправильний логін або пароль");
        }
    }
}
