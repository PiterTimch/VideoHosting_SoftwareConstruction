using Application.Mappings;
using Application.Models.Account;
using Application.Models.Comments;
using Application.Models.Portfolio;
using Application.Models.User;
using Application.Models.Video;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Xunit;

namespace UnitTests.Mappings;

public class MappingsTests
{
    private readonly VideoMappingProfile _videoMapper = new();
    private readonly CommentMappingProfile _commentMapper = new();
    private readonly UserMapping _userMapper = new();
    private readonly PortfolioMappingProfile _portfolioMapper = new();

    [Fact]
    public void VideoMappingProfile_MapsCorrectly()
    {
        var entity = new VideoEntity
        {
            Id = 1,
            Title = "Title",
            Description = "Desc",
            Slug = "slug",
            Video = "video.mp4",
            Image = "img.webp",
            ViewCount = 100,
            PrivacyId = 1
        };

        var item = _videoMapper.MapToItemModel(entity);
        item.Id.Should().Be(1);
        item.Title.Should().Be("Title");
        item.Description.Should().Be("Desc");

        var createModel = new VideoCreateModel { Title = "Created", Description = "Created Desc" };
        var createdEntity = _videoMapper.MapToEntity(createModel);
        createdEntity.Title.Should().Be("Created");

        var updateModel = new VideoUpdateModel { Id = 1, Title = "Updated" };
        _videoMapper.MapToEntity(updateModel, entity);
        entity.Title.Should().Be("Updated");

        var seedModel = new VideoSeedModel { Title = "Seeded", Slug = "seeded-slug" };
        var seededEntity = _videoMapper.MapToEntity(seedModel);
        seededEntity.Title.Should().Be("Seeded");
    }

    [Fact]
    public void VideoMappingProfile_MapsPrivacyAndReactionsAndProjections()
    {
        var privacyEntity = new VideoPrivacyEntity { Id = 2, Name = "Private", SystemCode = "private" };
        var privacyModel = _videoMapper.MapToItemModel(privacyEntity);
        privacyModel.Id.Should().Be(2);
        privacyModel.Name.Should().Be("Private");
        privacyModel.SystemCode.Should().Be("private");

        var reactionModel = new VideoReactionModel { VideoId = 10, IsLike = true };
        var reactionEntity = _videoMapper.MapToEntity(reactionModel);
        reactionEntity.VideoId.Should().Be(10);
        reactionEntity.IsLike.Should().BeTrue();

        var dummyUser = new UserEntity { Id = 1, UserName = "reaction_user" };
        var videoWithReactions = new VideoEntity
        {
            Id = 3,
            Title = "Video with Reactions",
            PrivacyId = 2,
            DateCreated = new DateTime(2025, 5, 20, 10, 0, 0, DateTimeKind.Utc)
        };
        videoWithReactions.VideoReactions = new List<VideoReactionEntity>
        {
            new() { VideoId = 3, UserId = 1, User = dummyUser, Video = videoWithReactions, IsLike = true },
            new() { VideoId = 3, UserId = 1, User = dummyUser, Video = videoWithReactions, IsLike = false },
            new() { VideoId = 3, UserId = 1, User = dummyUser, Video = videoWithReactions, IsLike = true }
        };

        var mapped = _videoMapper.MapToItemModel(videoWithReactions);
        typeof(VideoMappingProfile)
            .GetMethod("AfterMapToItemModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_videoMapper, new object[] { videoWithReactions, mapped });
        mapped.LikesCount.Should().Be(2);
        mapped.DislikesCount.Should().Be(1);
        mapped.DateCreated.Should().Be("20.05.2025р.");

        var projectedVideos = _videoMapper.ProjectToItemModel(new[] { videoWithReactions }.AsQueryable()).ToList();
        projectedVideos.Should().HaveCount(1);
        projectedVideos[0].LikesCount.Should().Be(2);
        projectedVideos[0].DislikesCount.Should().Be(1);

        var projectedPrivacies = _videoMapper.ProjectToItemModel(new[] { privacyEntity }.AsQueryable()).ToList();
        projectedPrivacies.Should().HaveCount(1);
        projectedPrivacies[0].SystemCode.Should().Be("private");
    }

    [Fact]
    public void CommentMappingProfile_MapsCorrectly()
    {
        var user = new UserEntity { UserName = "john", Image = "avatar.jpg" };
        var entity = new CommentsEntity
        {
            Id = 5,
            Content = "Test comment",
            User = user,
            UserId = 1,
            Replies = new List<CommentsEntity> { new() { Id = 6 } }
        };

        var item = _commentMapper.MapToItemModel(entity);
        item.Id.Should().Be(5);
        item.Content.Should().Be("Test comment");
        item.UserName.Should().Be("john");
        item.UserImage.Should().Be("avatar.jpg");
        item.RepliesCount.Should().Be(1);
    }

    [Fact]
    public void UserMapping_MapsCorrectly()
    {
        var regModel = new AccountRegisterModel
        {
            Email = "reg@test.com",
            FirstName = "First",
            LastName = "Last"
        };
        var regEntity = _userMapper.MapToEntity(regModel);
        regEntity.Email.Should().Be("reg@test.com");
        regEntity.FirstName.Should().Be("First");

        var seedModel = new UserSeedModel
        {
            Email = "seed@test.com",
            FirstName = "SeedFirst",
            LastName = "SeedLast"
        };
        var seedEntity = _userMapper.MapToEntity(seedModel);
        seedEntity.Email.Should().Be("seed@test.com");

        var editModel = new UserEditModel { FirstName = "NewName" };
        _userMapper.MapToEntity(editModel, regEntity);
        regEntity.FirstName.Should().Be("NewName");
    }

    [Fact]
    public void UserMapping_ItemModelAndProjections_MapsRolesLoginsAndFlags()
    {
        var role = new RoleEntity { Id = 1, Name = "Admin" };
        var user = new UserEntity
        {
            Id = 1,
            UserName = "user1@test.com",
            Email = "user1@test.com",
            PasswordHash = "some_hash"
        };
        user.UserRoles = new List<UserRoleEntity>
        {
            new() { UserId = 1, RoleId = 1, Role = role, User = user }
        };
        user.UserLogins = new List<UserLoginEntity>
        {
            new() { UserId = 1, LoginProvider = "Google", ProviderKey = "google-id", User = user }
        };

        var item = _userMapper.MapToItemModel(user);
        typeof(UserMapping)
            .GetMethod("AfterMapToItemModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_userMapper, new object[] { user, item });
        item.Roles.Should().Contain("Admin");
        item.LoginTypes.Should().Contain("Google");
        item.IsLoginGoogle.Should().BeTrue();
        item.IsLoginPassword.Should().BeTrue();

        var emptyUser = new UserEntity
        {
            Id = 2,
            UserName = "empty@test.com",
            PasswordHash = null,
            UserRoles = new List<UserRoleEntity>(),
            UserLogins = new List<UserLoginEntity>()
        };
        var emptyItem = _userMapper.MapToItemModel(emptyUser);
        typeof(UserMapping)
            .GetMethod("AfterMapToItemModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(_userMapper, new object[] { emptyUser, emptyItem });
        emptyItem.IsLoginGoogle.Should().BeFalse();
        emptyItem.IsLoginPassword.Should().BeFalse();

        var projected = _userMapper.ProjectToItemModel(new[] { user }.AsQueryable()).ToList();
        projected.Should().HaveCount(1);
        projected[0].IsLoginGoogle.Should().BeTrue();
        projected[0].IsLoginPassword.Should().BeTrue();
        projected[0].Roles.Should().Contain("Admin");
        projected[0].LoginTypes.Should().Contain("Google");
    }

    [Fact]
    public void PortfolioMappingProfile_MapsCorrectly()
    {
        var user = new UserEntity { Id = 10, UserName = "freelancer_nick", Image = "f.jpg" };
        var entity = new PortfolioEntity
        {
            Id = 10,
            Name = "My Portfolio",
            NickName = "my_nick",
            Description = "Dev",
            Freelancer = user
        };

        var item = _portfolioMapper.MapToItemModel(entity);
        item.Id.Should().Be(10);
        item.Name.Should().Be("My Portfolio");
        item.NickName.Should().Be("my_nick");
    }
}
