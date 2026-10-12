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
