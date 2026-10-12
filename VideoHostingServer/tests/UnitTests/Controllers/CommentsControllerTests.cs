using Application.Features.Comments.Commands.CreateComments;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Queries.GetCommentsReplies;
using Application.Features.Comments.Queries.GetVideoComments;
using Application.Models.Comments;
using Application.Models.Search;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VideoHostingServer.Controllers;
using Xunit;

namespace UnitTests.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CommentsController _controller;

    public CommentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CommentsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetByVideo_ValidId_ReturnsOkWithSearchResult()
    {
        var expected = new SearchResult<CommentsItemModal>
        {
            Items = new List<CommentsItemModal> { new() { Id = 1, Content = "Great video!" } }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVideoCommentsQuery>(), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetByVideo(10, new BaseSearchParamsModel());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task GetReplies_ValidParentId_ReturnsOkWithSearchResult()
    {
        var expected = new SearchResult<CommentsItemModal>
        {
            Items = new List<CommentsItemModal> { new() { Id = 2, Content = "Thanks!" } }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCommentRepliesQuery>(), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetReplies(1, new BaseSearchParamsModel());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsOkWithCreatedComment()
    {
        var request = new CreateCommentRequest("Nice video", 10, null);
        var expected = new CommentsItemModal { Id = 1, Content = "Nice video" };
        _mediatorMock.Setup(m => m.Send(It.Is<CreateCommentCommand>(c => c.Content == "Nice video" && c.VideoId == 10 && c.ParentId == null), default))
            .ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Update_ValidRequest_ReturnsOkWithUpdatedComment()
    {
        var request = new VideoHostingServer.Controllers.UpdateCommentRequest("Edited comment");
        var expected = new CommentsItemModal { Id = 5, Content = "Edited comment" };
        _mediatorMock.Setup(m => m.Send(It.Is<UpdateCommentCommand>(c => c.Id == 5 && c.Content == "Edited comment"), default))
            .ReturnsAsync(expected);

        var result = await _controller.Update(5, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteCommentCommand>(c => c.Id == 7), default))
            .ReturnsAsync(Unit.Value);

        var result = await _controller.Delete(7);

        result.Should().BeOfType<NoContentResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteCommentCommand>(c => c.Id == 7), default), Times.Once);
    }
}
