using Application.Models.User;
using FluentAssertions;
using MediatR;
using Moq;
using VideoHostingServer.Controllers;
using Xunit;

namespace UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task SearchUsers_ThrowsNotImplementedException()
    {
        await FluentActions.Invoking(() => _controller.SearchUsers(new UserSearchModel()))
            .Should().ThrowAsync<NotImplementedException>()
            .WithMessage("SearchUsers query is missing in Application layer.");
    }

    [Fact]
    public async Task EditUser_ThrowsNotImplementedException()
    {
        await FluentActions.Invoking(() => _controller.EditUser(new UserEditModel()))
            .Should().ThrowAsync<NotImplementedException>()
            .WithMessage("EditUser command is missing in Application layer.");
    }

    [Fact]
    public async Task DeleteUser_ThrowsNotImplementedException()
    {
        await FluentActions.Invoking(() => _controller.DeleteUser(new UserDeleteModel()))
            .Should().ThrowAsync<NotImplementedException>()
            .WithMessage("DeleteUser command is missing in Application layer.");
    }
}
