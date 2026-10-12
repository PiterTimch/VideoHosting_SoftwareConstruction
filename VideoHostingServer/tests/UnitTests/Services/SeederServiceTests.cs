using System.Text.Json;
using Application.Constants;
using Application.Interfaces;
using Application.Mappings;
using Application.Models.User;
using Application.Models.Video;
using Application.Services;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.Services;

public class SeederServiceTests
{
    private readonly Mock<RoleManager<RoleEntity>> _roleManagerMock;
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly Mock<IImageService> _imageServiceMock;
    private readonly Mock<IVideoFileService> _videoFileServiceMock;
    private readonly VideoMappingProfile _videoMapper = new();
    private readonly UserMapping _userMapper = new();

    public SeederServiceTests()
    {
        var roleStore = new Mock<IRoleStore<RoleEntity>>();
        _roleManagerMock = new Mock<RoleManager<RoleEntity>>(roleStore.Object, null, null, null, null);

        var userStore = new Mock<IUserStore<UserEntity>>();
        _userManagerMock = new Mock<UserManager<UserEntity>>(userStore.Object, null, null, null, null, null, null, null, null);

        _imageServiceMock = new Mock<IImageService>();
        _videoFileServiceMock = new Mock<IVideoFileService>();
    }

    [Fact]
    public async Task SeedRolesAsync_CreatesMissingRoles()
    {
        var context = TestDbContextHelper.CreateContext();
        _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<RoleEntity>())).ReturnsAsync(IdentityResult.Success);

        var service = new SeederService(
            context,
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _videoMapper,
            _userMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object);

        await service.SeedRolesAsync();

        foreach (var role in Roles.AllRoles)
        {
            _roleManagerMock.Verify(r => r.RoleExistsAsync(role), Times.Once);
            _roleManagerMock.Verify(r => r.CreateAsync(It.Is<RoleEntity>(re => re.Name == role)), Times.Once);
        }
    }

    [Fact]
    public async Task SeedVideoPrivaciesAsync_WhenEmpty_SeedsPrivacies()
    {
        var context = TestDbContextHelper.CreateContext();
        var service = new SeederService(
            context,
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _videoMapper,
            _userMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object);

        await service.SeedVideoPrivaciesAsync();

        var privacies = await context.VideoPrivacies.ToListAsync();
        privacies.Should().HaveCount(3);
        privacies.Select(p => p.SystemCode).Should().Contain([VideoPrivacyConstants.Public, VideoPrivacyConstants.Private, VideoPrivacyConstants.UrlOnly]);

        await service.SeedVideoPrivaciesAsync();
        (await context.VideoPrivacies.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task SeedUsersAsync_WhenFileNotFoundOrUsersExist_ReturnsSafely()
    {
        var context = TestDbContextHelper.CreateContext();
        var service = new SeederService(
            context,
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _videoMapper,
            _userMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object);

        await service.SeedUsersAsync("non_existent_file.json");

        context.Users.Add(new UserEntity { Id = 1, UserName = "existing" });
        await context.SaveChangesAsync();

        await service.SeedUsersAsync("dummy.json");
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SeedUsersAsync_WithValidJson_CreatesUsersAndPortfolios()
    {
        var context = TestDbContextHelper.CreateContext();
        var tempFile = Path.GetTempFileName();
        try
        {
            var seedUsers = new List<UserSeedModel>
            {
                new()
                {
                    Email = "admin@site.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Password = "Password123!",
                    Roles = new List<string> { "Admin" },
                    ImagePath = "http://example.com/admin.jpg"
                }
            };
            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(seedUsers));

            _imageServiceMock.Setup(i => i.SaveImageFromUrlAsync(It.IsAny<string>()))
                .ReturnsAsync("admin_saved.webp");
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync("Admin")).ReturnsAsync(true);

            var service = new SeederService(
                context,
                _roleManagerMock.Object,
                _userManagerMock.Object,
                _videoMapper,
                _userMapper,
                _imageServiceMock.Object,
                _videoFileServiceMock.Object);

            await service.SeedUsersAsync(tempFile);

            _userManagerMock.Verify(u => u.CreateAsync(It.Is<UserEntity>(e => e.Email == "admin@site.com"), "Password123!"), Times.Once);
            var portfolios = await context.Portfolios.ToListAsync();
            portfolios.Should().HaveCount(1);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SeedVideosAsync_WhenFileDoesNotExist_ReturnsSafely()
    {
        var context = TestDbContextHelper.CreateContext();
        var service = new SeederService(
            context,
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _videoMapper,
            _userMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object);

        await service.SeedVideosAsync("non_existent_videos.json", "dummy_folder");

        (await context.Videos.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task SeedVideosAsync_WithValidJson_SeedsVideosAndProcessesMedia()
    {
        var context = TestDbContextHelper.CreateContext();
        var publicPrivacy = new VideoPrivacyEntity { Id = 1, Name = "Public", SystemCode = VideoPrivacyConstants.Public };
        var privatePrivacy = new VideoPrivacyEntity { Id = 2, Name = "Private", SystemCode = VideoPrivacyConstants.Private };
        context.VideoPrivacies.AddRange(publicPrivacy, privatePrivacy);

        var portfolio = new Domain.Entities.Portfolio.PortfolioEntity { Id = 10, Name = "P10", NickName = "p10" };
        context.Portfolios.Add(portfolio);

        // Pre-existing video with slug "existing-slug" to test duplicate skip branch
        context.Videos.Add(new VideoEntity { Id = 1, Title = "Existing", Slug = "existing-slug" });
        await context.SaveChangesAsync();

        var tempDir = Path.Combine(Path.GetTempPath(), "SeederTest_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        var jsonFile = Path.Combine(tempDir, "videos.json");
        var videoLocalFile = Path.Combine(tempDir, "clip.mp4");
        await File.WriteAllTextAsync(videoLocalFile, "dummy video data");

        try
        {
            var seedVideos = new List<VideoSeedModel>
            {
                new()
                {
                    Title = "Video 1",
                    Slug = "video-1",
                    PrivacySystemCode = VideoPrivacyConstants.Private,
                    ImagePath = "http://example.com/pic1.jpg",
                    VideoFile = "clip.mp4"
                },
                new()
                {
                    Title = "Video 2 - Fallback Privacy & Throwing Image",
                    Slug = "video-2",
                    PrivacySystemCode = "NON_EXISTENT_CODE",
                    ImagePath = "http://example.com/badpic.jpg",
                    VideoFile = "missing.mp4"
                },
                new()
                {
                    Title = "Duplicate Slug Video",
                    Slug = "existing-slug"
                }
            };
            await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(seedVideos));

            _imageServiceMock.Setup(i => i.SaveImageFromUrlAsync("http://example.com/pic1.jpg"))
                .ReturnsAsync("pic1.webp");
            _imageServiceMock.Setup(i => i.SaveImageFromUrlAsync("http://example.com/badpic.jpg"))
                .ThrowsAsync(new HttpRequestException("Not found"));
            _videoFileServiceMock.Setup(v => v.SaveVideoFromFilePathAsync(videoLocalFile))
                .ReturnsAsync("saved_video.mp4");

            var service = new SeederService(
                context,
                _roleManagerMock.Object,
                _userManagerMock.Object,
                _videoMapper,
                _userMapper,
                _imageServiceMock.Object,
                _videoFileServiceMock.Object);

            await service.SeedVideosAsync(jsonFile, tempDir);

            var videos = await context.Videos.ToListAsync();
            videos.Should().HaveCount(3); // 1 existing + 2 newly seeded (the duplicate was skipped)

            var v1 = videos.First(v => v.Slug == "video-1");
            v1.PrivacyId.Should().Be(2);
            v1.PortfolioId.Should().Be(10);
            v1.Image.Should().Be("pic1.webp");
            v1.Video.Should().Be("saved_video.mp4");

            var v2 = videos.First(v => v.Slug == "video-2");
            v2.PrivacyId.Should().Be(1); // Fallback to public
            v2.PortfolioId.Should().Be(10);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task SeedRolesAsync_WhenRoleCreationFails_HandlesErrorGracefully()
    {
        var context = TestDbContextHelper.CreateContext();
        _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<RoleEntity>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role creation failed" }));

        var service = new SeederService(
            context,
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _videoMapper,
            _userMapper,
            _imageServiceMock.Object,
            _videoFileServiceMock.Object);

        await service.SeedRolesAsync();
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<RoleEntity>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SeedUsersAsync_EmptyJsonOrUserCreationFailsOrImageThrows_HandlesGracefully()
    {
        var context = TestDbContextHelper.CreateContext();
        var tempFile = Path.GetTempFileName();
        try
        {
            // Empty list
            await File.WriteAllTextAsync(tempFile, "[]");
            var service = new SeederService(
                context,
                _roleManagerMock.Object,
                _userManagerMock.Object,
                _videoMapper,
                _userMapper,
                _imageServiceMock.Object,
                _videoFileServiceMock.Object);

            await service.SeedUsersAsync(tempFile);

            // User with throwing image and failing creation
            var users = new List<UserSeedModel>
            {
                new()
                {
                    Email = "fail@test.com",
                    Password = "Pass",
                    ImagePath = "http://bad.url",
                    Roles = ["Admin", "UnknownRole"]
                }
            };
            await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(users));
            _imageServiceMock.Setup(i => i.SaveImageFromUrlAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Network error"));
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            await service.SeedUsersAsync(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
