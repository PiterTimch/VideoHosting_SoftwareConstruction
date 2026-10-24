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
using Application.Constants;
using Application.Interfaces;
using Application.Jobs;
using Application.Mappings;
using Application.Models.Search;
using Application.Models.Video;
using Application.Models.VideoProcessing;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Hangfire;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Handlers;

public class VideoHandlersTests
{
    private readonly VideoMappingProfile _videoMapper = new();
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<IVideoFileService> _videoFileServiceMock = new();
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock = new();
    private readonly Mock<IVideoProgressStore> _progressStoreMock = new();
    private readonly Mock<ILogger<CreateVideoHandler>> _createLoggerMock = new();
    private readonly Mock<ILogger<UpdateVideoHandler>> _updateLoggerMock = new();
    private readonly Mock<IVideoRecommendationService> _recommendationServiceMock = new();

    private static (UserEntity user, PortfolioEntity portfolio, VideoPrivacyEntity privacy) SeedRelations(Domain.AppDbContext context)
    {
        var user = new UserEntity { Id = 1, UserName = "User1" };
        var portfolio = new PortfolioEntity { Id = 1, Name = "Port1", NickName = "port1", Freelancer = user };
        var privacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = "public" };
        context.Users.Add(user);
        context.Portfolios.Add(portfolio);
        context.VideoPrivacies.Add(privacy);
        return (user, portfolio, privacy);
    }

    [Fact]
    public async Task CreateVideoHandler_CreatesVideoAndQueuesJob()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.mp4");

        var model = new VideoCreateModel
        {
            Title = "New Test Video",
            Description = "Desc",
            Video = fileMock.Object
        };

        var handler = new CreateVideoHandler(
            repo,
            _videoMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object,
            _progressStoreMock.Object,
            _createLoggerMock.Object);

        var result = await handler.Handle(new CreateVideoCommand(model), default);

        result.TrackingId.Should().NotBeNullOrWhiteSpace();
        _progressStoreMock.Verify(p => p.Set(result.TrackingId, It.IsAny<VideoProgressUpdate>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVideoHandler_WhenVideoExists_Deletes()
    {
        var context = TestDbContextHelper.CreateContext();
        var video = new VideoEntity { Id = 1, Title = "To Delete", IsDeleted = false };
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new DeleteVideoHandler(repo);

        await handler.Handle(new DeleteVideoCommand(new VideoDeleteModel { Id = 1 }), default);

        var deleted = await context.Videos.FindAsync(1L);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteVideoHandler_WhenVideoDoesNotExist_ReturnsSafely()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new DeleteVideoHandler(repo);

        await handler.Handle(new DeleteVideoCommand(new VideoDeleteModel { Id = 999 }), default);
    }

    [Fact]
    public async Task UpdateVideoHandler_WhenVideoNotFound_ThrowsException()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new UpdateVideoHandler(
            repo, _videoMapper, _imageServiceMock.Object, _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object, _progressStoreMock.Object, _updateLoggerMock.Object);

        await FluentActions.Invoking(() => handler.Handle(new UpdateVideoCommand(new VideoUpdateModel { Id = 999 }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task UpdateVideoHandler_WhenAlreadyProcessing_ThrowsInvalidOperationException()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Processing", Video = "processing...", IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new UpdateVideoHandler(
            repo, _videoMapper, _imageServiceMock.Object, _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object, _progressStoreMock.Object, _updateLoggerMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.mp4");

        await FluentActions.Invoking(() => handler.Handle(new UpdateVideoCommand(new VideoUpdateModel { Id = 1, Video = fileMock.Object }), default))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("Відео вже обробляється. Зачекайте завершення попереднього завантаження.");
    }

    [Fact]
    public async Task UpdateVideoHandler_UpdatesImagesAndFieldsSuccessfully()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Old Title", Image = "old.jpg", IsDeleted = false });
        await context.SaveChangesAsync();

        _imageServiceMock.Setup(i => i.SaveImageAsync(It.IsAny<IFormFile>())).ReturnsAsync("new.webp");

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new UpdateVideoHandler(
            repo, _videoMapper, _imageServiceMock.Object, _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object, _progressStoreMock.Object, _updateLoggerMock.Object);

        var imgMock = new Mock<IFormFile>();
        var result = await handler.Handle(new UpdateVideoCommand(new VideoUpdateModel { Id = 1, Title = "New Title", Image = imgMock.Object }), default);

        result.TrackingId.Should().NotBeNullOrWhiteSpace();
        _imageServiceMock.Verify(i => i.DeleteImageAsync("old.jpg"), Times.Once);
        var updated = await context.Videos.FindAsync(1L);
        updated!.Title.Should().Be("New Title");
        updated.Image.Should().Be("new.webp");
    }

    [Fact]
    public async Task AutocompleteVideosQueryHandler_WorksCorrectly()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "React Framework Tutorial", Slug = "react-tut", Video = "ready.mp4", IsDeleted = false });
        context.Videos.Add(new VideoEntity { Id = 2, Title = "Vue JS", Slug = "vue", Video = "ready.mp4", IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new AutocompleteVideosQueryHandler(repo);

        var empty = await handler.Handle(new AutocompleteVideosQuery("   "), default);
        empty.Should().BeEmpty();

        var match = await handler.Handle(new AutocompleteVideosQuery("react"), default);
        match.Should().HaveCount(1);
        match.First().Title.Should().Be("React Framework Tutorial");
    }

    [Fact]
    public async Task GetByVideoQueryHandler_ByIdAndBySlug_ReturnsVideo()
    {
        var context = TestDbContextHelper.CreateContext();
        var (_, portfolio, privacy) = SeedRelations(context);
        context.Videos.Add(new VideoEntity { Id = 1, Title = "My Video", Slug = "my-video", PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        currentUser.Setup(c => c.TryGetCurrentUserId()).Returns(1L);
        var handler = new GetByVideoQueryHandler(repo, _videoMapper, currentUser.Object, Mock.Of<IVideoAccessTokenService>());

        var byId = await handler.Handle(new GetByVideoQuery(new GetByModel { Id = 1 }), default);
        byId.Title.Should().Be("My Video");

        var bySlug = await handler.Handle(new GetByVideoQuery(new GetByModel { Slug = "my-video" }), default);
        bySlug.Title.Should().Be("My Video");

        await FluentActions.Invoking(() => handler.Handle(new GetByVideoQuery(new GetByModel()), default))
            .Should().ThrowAsync<Exception>().WithMessage("Необхідно вказати Id або Slug");

        await FluentActions.Invoking(() => handler.Handle(new GetByVideoQuery(new GetByModel { Id = 999 }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task GetVideoPrivaciesQueryHandler_ReturnsAllPrivacies()
    {
        var context = TestDbContextHelper.CreateContext();
        context.VideoPrivacies.Add(new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = "public" });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoPrivacyEntity, long>(context);
        var handler = new GetVideoPrivaciesQueryHandler(repo, _videoMapper);

        var result = await handler.Handle(new GetVideoPrivaciesQuery(), default);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetVideosQueryHandler_ReturnsVideos()
    {
        var context = TestDbContextHelper.CreateContext();
        var (_, portfolio, privacy) = SeedRelations(context);
        context.Videos.Add(new VideoEntity { Id = 1, Title = "V1", Video = "v1.mp4", PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new GetVideosQueryHandler(repo, _videoMapper);

        var result = await handler.Handle(new GetVideosQuery(), default);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetVideoRecommendationsQueryHandler_ReturnsTopRecommendations()
    {
        var context = TestDbContextHelper.CreateContext();
        var (_, portfolio, privacy) = SeedRelations(context);
        var v1 = new VideoEntity { Id = 1, Title = "CSharp 12", Description = "Learn C#", Video = "ready.mp4", PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false };
        var v2 = new VideoEntity { Id = 2, Title = "CSharp 13", Description = "Learn C#", Video = "ready.mp4", PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false };
        var v3 = new VideoEntity { Id = 3, Title = "Unrelated", Description = "Cooking", Video = "ready.mp4", PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false };
        context.Videos.AddRange(v1, v2, v3);
        await context.SaveChangesAsync();

        _recommendationServiceMock.Setup(r => r.ComputeScore(It.Is<VideoEntity>(v => v.Id == 1), It.Is<VideoEntity>(v => v.Id == 2))).Returns(10);
        _recommendationServiceMock.Setup(r => r.ComputeScore(It.Is<VideoEntity>(v => v.Id == 1), It.Is<VideoEntity>(v => v.Id == 3))).Returns(0);

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new GetVideoRecommendationsQueryHandler(repo, _videoMapper, _recommendationServiceMock.Object);

        var result = await handler.Handle(new GetVideoRecommendationsQuery(new VideoRecommendationRequest { VideoId = 1 }), default);
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);

        await FluentActions.Invoking(() => handler.Handle(new GetVideoRecommendationsQuery(new VideoRecommendationRequest { VideoId = 999 }), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task SearchVideosQueryHandler_AppliesAllFiltersAndPaging()
    {
        var context = TestDbContextHelper.CreateContext();
        var (_, portfolio, privacy) = SeedRelations(context);
        var v1 = new VideoEntity { Id = 1, Title = "Dotnet Core", Description = "C# intro", Video = "v1.mp4", ViewCount = 50, DateCreated = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false };
        var v2 = new VideoEntity { Id = 2, Title = "Dotnet Advanced", Description = "C# async", Video = "v2.mp4", ViewCount = 500, DateCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), PrivacyId = 1, Privacy = privacy, PortfolioId = 1, Portfolio = portfolio, IsDeleted = false };
        context.Videos.AddRange(v1, v2);
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new SearchVideosQueryHandler(repo, _videoMapper);

        var resQ = await handler.Handle(new SearchVideosQuery(new VideoSearchModel { Q = "core" }), default);
        resQ.Items.Should().HaveCount(1);

        var resTitle = await handler.Handle(new SearchVideosQuery(new VideoSearchModel { Title = "advanced" }), default);
        resTitle.Items.Should().HaveCount(1);

        var resYear = await handler.Handle(new SearchVideosQuery(new VideoSearchModel { CreateYearFrom = "2023", CreateYearTo = "2025" }), default);
        resYear.Items.Should().HaveCount(1);
        resYear.Items.First().Id.Should().Be(2);

        var resViews = await handler.Handle(new SearchVideosQuery(new VideoSearchModel { SortBy = "views" }), default);
        resViews.Items.First().Id.Should().Be(2);

        var resDate = await handler.Handle(new SearchVideosQuery(new VideoSearchModel { SortBy = "date" }), default);
        resDate.Items.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task IncrementViewCommandHandler_IncrementsViewCount_OrThrowsWhenNotFound()
    {
        using var context = TestDbContextHelper.CreateSqliteContext();
        context.VideoPrivacies.Add(new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = "public" });
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Viewed", Slug = "viewed", PrivacyId = 1, ViewCount = 5, IsDeleted = false });
        context.Videos.Add(new VideoEntity { Id = 2, Title = "Deleted", Slug = "deleted", PrivacyId = 1, ViewCount = 0, IsDeleted = true });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new IncrementViewCommandHandler(repo);

        await handler.Handle(new IncrementViewCommand(1), default);
        context.ChangeTracker.Clear();
        var updated = await context.Videos.FindAsync(1L);
        updated!.ViewCount.Should().Be(6);

        await FluentActions.Invoking(() => handler.Handle(new IncrementViewCommand(2), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");

        await FluentActions.Invoking(() => handler.Handle(new IncrementViewCommand(999), default))
            .Should().ThrowAsync<Exception>().WithMessage("Відео не знайдено");
    }

    [Fact]
    public async Task UpdateVideoHandler_ReplacesVideoFile_DeletesOldAndQueuesJob()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Old Title", Video = "old_vid.mp4", IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new UpdateVideoHandler(
            repo, _videoMapper, _imageServiceMock.Object, _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object, _progressStoreMock.Object, _updateLoggerMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("new_clip.mp4");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.Handle(new UpdateVideoCommand(new VideoUpdateModel
        {
            Id = 1,
            Title = "Updated Title",
            Video = fileMock.Object
        }), default);

        result.TrackingId.Should().NotBeNullOrWhiteSpace();
        _videoFileServiceMock.Verify(v => v.DeleteVideoAsync("old_vid.mp4"), Times.Once);
        _progressStoreMock.Verify(p => p.Set(result.TrackingId, It.Is<VideoProgressUpdate>(u => u.Status == "В черзі")), Times.Once);
        _backgroundJobClientMock.Verify(b => b.Create(
            It.IsAny<Hangfire.Common.Job>(),
            It.IsAny<Hangfire.States.IState>()), Times.Once);

        var videoInDb = await context.Videos.FindAsync(1L);
        videoInDb!.Video.Should().Be("processing...");
    }

    [Fact]
    public async Task UpdateVideoHandler_WhenUkrainianProcessingStatus_ThrowsInvalidOperationException()
    {
        var context = TestDbContextHelper.CreateContext();
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Processing", Video = "обробляється...", IsDeleted = false });
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context);
        var handler = new UpdateVideoHandler(
            repo, _videoMapper, _imageServiceMock.Object, _videoFileServiceMock.Object,
            _backgroundJobClientMock.Object, _progressStoreMock.Object, _updateLoggerMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("clip.mp4");

        await FluentActions.Invoking(() => handler.Handle(new UpdateVideoCommand(new VideoUpdateModel { Id = 1, Video = fileMock.Object }), default))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("Відео вже обробляється. Зачекайте завершення попереднього завантаження.");
    }
}
