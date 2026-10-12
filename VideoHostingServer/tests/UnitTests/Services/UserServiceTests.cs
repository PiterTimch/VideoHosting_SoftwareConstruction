using Application.Models.User;
using Application.Services;
using Domain.Entities.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task LoadLoginsAndRolesAsync_PopulatesLoginTypesAndRoles()
    {
        var context = TestDbContextHelper.CreateContext();
        context.UserLogins.Add(new UserLoginEntity { UserId = 1, LoginProvider = "Google", ProviderKey = "123" });
        await context.SaveChangesAsync();

        var store = new Mock<IUserStore<UserEntity>>();
        var userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);

        var userEntity1 = new UserEntity { Id = 1, PasswordHash = "some_hash" };
        var userEntity2 = new UserEntity { Id = 2, PasswordHash = null };
        var userList = new List<UserEntity> { userEntity1, userEntity2 }.BuildMockDbSet();

        userManagerMock.Setup(u => u.Users).Returns(userList.Object);
        userManagerMock.Setup(u => u.GetRolesAsync(userEntity1)).ReturnsAsync(new List<string> { "Admin", "User" });
        userManagerMock.Setup(u => u.GetRolesAsync(userEntity2)).ReturnsAsync(new List<string> { "User" });

        var service = new UserService(context, userManagerMock.Object);

        var model1 = new UserItemModel { Id = 1 };
        var model2 = new UserItemModel { Id = 2 };
        var users = new List<UserItemModel> { model1, model2 };

        await service.LoadLoginsAndRolesAsync(users);

        model1.LoginTypes.Should().Contain("Google");
        model1.LoginTypes.Should().Contain("Password");
        model1.Roles.Should().Contain("Admin");
        model1.Roles.Should().Contain("User");

        model2.LoginTypes.Should().NotContain("Password");
        model2.Roles.Should().Contain("User");
    }
}
