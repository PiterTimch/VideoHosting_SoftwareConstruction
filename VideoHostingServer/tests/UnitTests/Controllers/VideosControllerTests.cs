using Application.Features.Videos.Commands.CreateVideo;
using Application.Features.Videos.Commands.DeleteVideo;
using Application.Features.Videos.Commands.IncrementView;
using Application.Features.Videos.Commands.UpdateVideo;
using Application.Features.Videos.Queries.AutocompleteVideos;
using Application.Features.Videos.Queries.GetByVideo;
using Application.Features.Videos.Queries.GetVideoPrivacies;
using Application.Features.Videos.Queries.GetVideoRecommendations;
using Application.Features.Videos.Queries.GetVideos;
using Application.Features.Videos.Queries.SearchVideos;
using Application.Interfaces;
using Application.Models.Search;
using Application.Models.Video;
using Application.Models.VideoProcessing;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VideoHostingServer.Controllers;
using Xunit;

namespace UnitTests.Controllers;

public class VideosControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IVideoFileService> _videoFileServiceMock;
    private readonly Mock<IVideoProgressStore> _progressStoreMock;
    private readonly Mock<ILogger<VideosController>> _loggerMock;
    private readonly VideosController _controller;

    public VideosControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _videoFileServiceMock = new Mock<IVideoFileService>();
        _progressStoreMock = new Mock<IVideoProgressStore>();
        _loggerMock = new Mock<ILogger<VideosController>>();

        _controller = new VideosController(
            _mediatorMock.Object,
            _videoFileServiceMock.Object,
            _progressStoreMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task TestVideoSaving_CallsVideoFileService_ReturnsOk()
    {
        var fileMock = new Mock<IFormFile>();
        _videoFileServiceMock.Setup(v => v.SaveVideoAsync(fileMock.Object))
            .ReturnsAsync("saved_video.mp4");

        var result = await _controller.TestVideoSaving(new TestVideoSavingCommand { File = fileMock.Object });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be("saved_video.mp4");
    }

    [Fact]
    public async Task GetAll_SendsGetVideosQuery_ReturnsOk()
    {
        var expected = new List<VideoItemModel> { new() { Id = 1, Title = "V1" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVideosQuery>(), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task GetById_SendsGetByVideoQuery_ReturnsOk()
    {
        var expected = new VideoItemModel { Id = 2, Title = "V2" };
        var model = new GetByModel { Id = 2 };
        _mediatorMock.Setup(m => m.Send(It.Is<GetByVideoQuery>(q => q.Model == model), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetById(model, null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Search_SendsSearchVideosQuery_ReturnsOk()
    {
        var expected = new SearchResult<VideoItemModel>
        {
            Items = new List<VideoItemModel> { new() { Id = 3 } }
        };
        var model = new VideoSearchModel { Q = "test" };
        _mediatorMock.Setup(m => m.Send(It.Is<SearchVideosQuery>(q => q.Model == model), default))
            .ReturnsAsync(expected);

        var result = await _controller.Search(model);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Create_SendsCreateVideoCommand_ReturnsOk()
    {
        var expected = new VideoProcessingResult { TrackingId = "track_123" };
        var model = new VideoCreateModel { Title = "New Video" };
        _mediatorMock.Setup(m => m.Send(It.Is<CreateVideoCommand>(c => c.Model == model), default))
            .ReturnsAsync(expected);

        var result = await _controller.Create(model);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Update_SendsUpdateVideoCommand_ReturnsOk()
    {
        var expected = new VideoProcessingResult { TrackingId = "track_upd" };
        var model = new VideoUpdateModel { Id = 1, Title = "Updated Video" };
        _mediatorMock.Setup(m => m.Send(It.Is<UpdateVideoCommand>(c => c.Model == model), default))
            .ReturnsAsync(expected);

        var result = await _controller.Update(model);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Delete_SendsDeleteVideoCommand_ReturnsOk()
    {
        var model = new VideoDeleteModel { Id = 1 };

        var result = await _controller.Delete(model);

        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteVideoCommand>(c => c.Model == model), default), Times.Once);
    }

    [Fact]
    public async Task GetPrivacies_SendsGetVideoPrivaciesQuery_ReturnsOk()
    {
        var expected = new List<VideoPrivacyItemModel> { new() { Id = 1, Name = "Public" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVideoPrivaciesQuery>(), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetPrivacies();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task IncrementView_SendsIncrementViewCommand_ReturnsOk()
    {
        var result = await _controller.IncrementView(100);

        result.Should().BeOfType<OkResult>();
        _mediatorMock.Verify(m => m.Send(It.Is<IncrementViewCommand>(c => c.Id == 100), default), Times.Once);
    }

    [Fact]
    public async Task React_ReturnsOk()
    {
        var result = await _controller.React(new VideoReactionModel { VideoId = 1, IsLike = true });

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetRecommendations_SendsGetVideoRecommendationsQuery_ReturnsOk()
    {
        var expected = new List<VideoItemModel> { new() { Id = 5, Title = "Recommended" } };
        var model = new VideoRecommendationRequest { VideoId = 1 };
        _mediatorMock.Setup(m => m.Send(It.Is<GetVideoRecommendationsQuery>(q => q.Model == model), default))
            .ReturnsAsync(expected);

        var result = await _controller.GetRecommendations(model);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public async Task Autocomplete_SendsAutocompleteVideosQuery_ReturnsOk()
    {
        var expected = new List<VideoAutocompleteModel> { new() { Title = "React tutorial" } };
        _mediatorMock.Setup(m => m.Send(It.Is<AutocompleteVideosQuery>(q => q.Q == "react"), default))
            .ReturnsAsync(expected);

        var result = await _controller.Autocomplete("react");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expected);
    }

    [Fact]
    public void GetProcessingProgress_WhenFound_ReturnsOkWithProgress()
    {
        var progress = new VideoProgressUpdate { Percentage = 75, Status = "Processing" };
        _progressStoreMock.Setup(p => p.Get("tid_123")).Returns(progress);

        var result = _controller.GetProcessingProgress("tid_123");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(progress);
    }

    [Fact]
    public void GetProcessingProgress_WhenNotFound_ReturnsNotFound()
    {
        _progressStoreMock.Setup(p => p.Get("tid_unknown")).Returns((VideoProgressUpdate?)null);

        var result = _controller.GetProcessingProgress("tid_unknown");

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
