using Application.Constants;
using Application.Features.Videos.Commands.GenerateVideoAccessToken;
using Application.Interfaces;
using Application.Services;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;

namespace UnitTests.Handlers;

public class GenerateVideoAccessTokenHandlerTests
{
    private static (
        Mock<IGenericRepository<VideoEntity, long>> repo,
        Mock<ICurrentUserService> currentUser,
        IVideoAccessTokenService tokenService,
        Mock<IConfiguration> config)
        CreateDeps(VideoEntity? video = null)
    {
        var repo = new Mock<IGenericRepository<VideoEntity, long>>();
        var videos = (video != null ? new List<VideoEntity> { video } : new List<VideoEntity>())
            .BuildMockDbSet();
        repo.Setup(r => r.AsQurable(false)).Returns(videos.Object);

        var currentUser = new Mock<ICurrentUserService>();
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("TestKeyForHandlerTests1234567890AAAAAAAAAA=");

        var sectionMock = new Mock<Microsoft.Extensions.Configuration.IConfigurationSection>();
        sectionMock.Setup(s => s.Value).Returns("60");
        configMock.Setup(c => c.GetSection("VideoAccessToken")).Returns(sectionMock.Object);

        var tokenService = new VideoAccessTokenService(configMock.Object);
        return (repo, currentUser, tokenService, configMock);
    }

    private static VideoEntity BuildVideo(long ownerId) => new()
    {
        Id = 1,
        Title = "T",
        PrivacyId = 1,
        Privacy = new VideoPrivacyEntity { Id = 1, Name = "Url", SystemCode = VideoPrivacyConstants.UrlOnly },
        Portfolio = new PortfolioEntity { Id = ownerId, Name = "P", NickName = "p", Freelancer = new UserEntity { Id = ownerId } },
        PortfolioId = ownerId,
        IsDeleted = false,
    };

    [Fact]
    public async Task Handle_Owner_ReturnsToken()
    {
        var (repo, currentUser, tokenService, config) = CreateDeps(BuildVideo(10));
        currentUser.Setup(c => c.TryGetCurrentUserId()).Returns(10L);
        currentUser.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);
        config.Setup(c => c.GetValue<int>("VideoAccessToken:ExpiryMinutes", 60)).Returns(60);

        var handler = new GenerateVideoAccessTokenHandler(repo.Object, currentUser.Object, tokenService, config.Object);
        var result = await handler.Handle(new GenerateVideoAccessTokenCommand(1L), CancellationToken.None);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_Admin_ReturnsToken()
    {
        var (repo, currentUser, tokenService, config) = CreateDeps(BuildVideo(10));
        currentUser.Setup(c => c.TryGetCurrentUserId()).Returns(99L);
        currentUser.Setup(c => c.IsInRole(Roles.Admin)).Returns(true);
        config.Setup(c => c.GetValue<int>("VideoAccessToken:ExpiryMinutes", 60)).Returns(60);

        var handler = new GenerateVideoAccessTokenHandler(repo.Object, currentUser.Object, tokenService, config.Object);
        var result = await handler.Handle(new GenerateVideoAccessTokenCommand(1L), CancellationToken.None);

        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsException()
    {
        var (repo, currentUser, tokenService, config) = CreateDeps(BuildVideo(10));
        currentUser.Setup(c => c.TryGetCurrentUserId()).Returns(999L);
        currentUser.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);

        var handler = new GenerateVideoAccessTokenHandler(repo.Object, currentUser.Object, tokenService, config.Object);

        await Assert.ThrowsAsync<Application.Exceptions.UnFreelancerizedAccessException>(
            () => handler.Handle(new GenerateVideoAccessTokenCommand(1L), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_VideoNotFound_ThrowsException()
    {
        var (repo, currentUser, tokenService, config) = CreateDeps();
        currentUser.Setup(c => c.TryGetCurrentUserId()).Returns(10L);
        currentUser.Setup(c => c.IsInRole(Roles.Admin)).Returns(false);

        var handler = new GenerateVideoAccessTokenHandler(repo.Object, currentUser.Object, tokenService, config.Object);

        await Assert.ThrowsAsync<Exception>(
            () => handler.Handle(new GenerateVideoAccessTokenCommand(9999L), CancellationToken.None));
    }
}

