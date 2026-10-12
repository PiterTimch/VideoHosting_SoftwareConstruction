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
}
