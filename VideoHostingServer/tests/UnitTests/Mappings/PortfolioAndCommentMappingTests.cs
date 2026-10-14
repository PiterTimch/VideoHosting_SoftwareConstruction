using Application.Mappings;
using Application.Models.Comments;
using Application.Models.Portfolio;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using FluentAssertions;
using Xunit;

namespace UnitTests.Mappings;

public class PortfolioAndCommentMappingTests
{
    // ── PortfolioMappingProfile ───────────────────────────────────────────────

    [Fact]
    public void PortfolioMappingProfile_MapToItemModel_MapsScalarFields()
    {
        var mapper = new PortfolioMappingProfile();
        var entity = new PortfolioEntity
        {
            Id = 5,
            Name = "Dev Portfolio",
            NickName = "devport",
            Description = "A great portfolio",
            Specialization = "Backend",
            Contacts = "contact@dev.com",
            Settings = "{\"theme\":\"dark\"}",
            AvatarImage = "avatar.jpg",
            BannerImage = "banner.jpg",
            Subscribers = new List<PortfolioSubscriberEntity>
            {
                new() { UserId = 1, User = new UserEntity { Id = 1, IsDeleted = false } },
                new() { UserId = 2, User = new UserEntity { Id = 2, IsDeleted = true } }, // deleted – not counted
            }
        };

        var result = mapper.MapToItemModel(entity);

        result.Id.Should().Be(5);
        result.Name.Should().Be("Dev Portfolio");
        result.NickName.Should().Be("devport");
        result.Description.Should().Be("A great portfolio");
        result.Specialization.Should().Be("Backend");
        result.Contacts.Should().Be("contact@dev.com");
        result.Settings.Should().Be("{\"theme\":\"dark\"}");
        result.AvatarImage.Should().Be("avatar.jpg");
        result.BannerImage.Should().Be("banner.jpg");
        result.SubscriberCount.Should().Be(1); // only non-deleted
    }

    [Fact]
    public void PortfolioMappingProfile_MapToItemModel_NullSubscribers_SubscriberCountIsZero()
    {
        var mapper = new PortfolioMappingProfile();
        var entity = new PortfolioEntity
        {
            Id = 1,
            Name = "P",
            NickName = "p",
            Subscribers = null
        };

        var result = mapper.MapToItemModel(entity);

        result.SubscriberCount.Should().Be(0);
    }

    [Fact]
    public void PortfolioMappingProfile_MapToItemModel_NoUserOnSubscriber_CountsIt()
    {
        var mapper = new PortfolioMappingProfile();
        var entity = new PortfolioEntity
        {
            Id = 1,
            Name = "P",
            NickName = "p",
            Subscribers = new List<PortfolioSubscriberEntity>
            {
                new() { UserId = 10, User = null } // null user → counts (not deleted)
            }
        };

        var result = mapper.MapToItemModel(entity);

        result.SubscriberCount.Should().Be(1);
    }

    [Fact]
    public void PortfolioMappingProfile_MapToEntity_Create_MapsCorrectFields()
    {
        var mapper = new PortfolioMappingProfile();
        var model = new PortfolioCreateModel
        {
            Name = "New Portfolio",
            NickName = "newport",
            Description = "desc",
            Specialization = "Frontend",
            Contacts = "me@me.com",
            Settings = null
        };

        var entity = mapper.MapToEntity(model);

        entity.Name.Should().Be("New Portfolio");
        entity.NickName.Should().Be("newport");
        entity.Description.Should().Be("desc");
        entity.Specialization.Should().Be("Frontend");
        entity.Contacts.Should().Be("me@me.com");
        // AvatarImage and BannerImage are ignored by mapper
        entity.AvatarImage.Should().BeNull();
        entity.BannerImage.Should().BeNull();
    }

    [Fact]
    public void PortfolioMappingProfile_MapToEntity_Update_UpdatesFields()
    {
        var mapper = new PortfolioMappingProfile();
        var model = new PortfolioUpdateModel
        {
            Id = 7,
            Name = "Updated",
            NickName = "updatednick",
            Description = "new desc"
        };
        var entity = new PortfolioEntity
        {
            Id = 7,
            Name = "Old",
            NickName = "old",
            AvatarImage = "old_avatar.jpg",
            BannerImage = "old_banner.jpg"
        };

        mapper.MapToEntity(model, entity);

        entity.Name.Should().Be("Updated");
        entity.NickName.Should().Be("updatednick");
        // avatar and banner unchanged (ignored by mapper)
        entity.AvatarImage.Should().Be("old_avatar.jpg");
        entity.BannerImage.Should().Be("old_banner.jpg");
    }

    [Fact]
    public void PortfolioMappingProfile_ProjectToItemModel_ProjectsCorrectly()
    {
        var mapper = new PortfolioMappingProfile();
        var entities = new[]
        {
            new PortfolioEntity
            {
                Id = 1,
                Name = "P1",
                NickName = "p1",
                Description = "d1",
                Subscribers = new List<PortfolioSubscriberEntity>
                {
                    new() { User = new UserEntity { IsDeleted = false } }
                }
            },
            new PortfolioEntity { Id = 2, Name = "P2", NickName = "p2", Description = "d2", Subscribers = null }
        }.AsQueryable();

        var results = mapper.ProjectToItemModel(entities).ToList();

        results.Should().HaveCount(2);
        results[0].Id.Should().Be(1);
        results[0].Name.Should().Be("P1");
        results[0].SubscriberCount.Should().Be(1);
        results[1].SubscriberCount.Should().Be(0);
    }

    // ── CommentMappingProfile ─────────────────────────────────────────────────

    [Fact]
    public void CommentMappingProfile_MapToItemModel_MapsAllFields()
    {
        var mapper = new CommentMappingProfile();
        var user = new UserEntity { Id = 3, UserName = "testuser", Image = "user.jpg" };
        var reply1 = new CommentsEntity { Id = 10, Content = "reply" };
        var entity = new CommentsEntity
        {
            Id = 1,
            Content = "Hello world",
            LikesCount = 5,
            DislikeCount = 2,
            IsEdited = true,
            UserId = 3,
            User = user,
            Replies = new List<CommentsEntity> { reply1 }
        };

        var result = mapper.MapToItemModel(entity);

        result.Id.Should().Be(1);
        result.Content.Should().Be("Hello world");
        result.LikesCount.Should().Be(5);
        result.DislikeCount.Should().Be(2);
        result.IsEdited.Should().BeTrue();
        result.UserId.Should().Be(3);
        result.UserName.Should().Be("testuser");
        result.UserImage.Should().Be("user.jpg");
        result.RepliesCount.Should().Be(1);
    }

    [Fact]
    public void CommentMappingProfile_MapToItemModel_EmptyReplies_RepliesCountIsZero()
    {
        var mapper = new CommentMappingProfile();
        var entity = new CommentsEntity
        {
            Id = 2,
            Content = "No replies",
            User = new UserEntity { UserName = "u" },
            Replies = new List<CommentsEntity>()
        };

        var result = mapper.MapToItemModel(entity);

        result.RepliesCount.Should().Be(0);
    }

    [Fact]
    public void CommentMappingProfile_ProjectToItemModel_ProjectsCollection()
    {
        var mapper = new CommentMappingProfile();
        var user = new UserEntity { Id = 1, UserName = "user1", Image = "img.png" };
        var reply = new CommentsEntity { Id = 10, Content = "r" };
        var entities = new[]
        {
            new CommentsEntity { Id = 1, Content = "c1", UserId = 1, User = user, Replies = new List<CommentsEntity> { reply } },
            new CommentsEntity { Id = 2, Content = "c2", UserId = 1, User = user, Replies = new List<CommentsEntity>() }
        }.AsQueryable();

        var results = mapper.ProjectToItemModel(entities).ToList();

        results.Should().HaveCount(2);
        results[0].Content.Should().Be("c1");
        results[0].UserName.Should().Be("user1");
        results[0].RepliesCount.Should().Be(1);
        results[1].Content.Should().Be("c2");
        results[1].RepliesCount.Should().Be(0);
    }
}
