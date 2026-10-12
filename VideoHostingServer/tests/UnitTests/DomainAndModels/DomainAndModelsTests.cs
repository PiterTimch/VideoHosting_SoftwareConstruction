using Application.Exceptions;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Portfolio.Commands.CreatePortfolio;
using Application.Models.Account;
using Application.Models.Comments;
using Application.Models.Portfolio;
using Application.Models.Search;
using Application.Models.User;
using Application.Models.Video;
using Application.Models.VideoProcessing;
using Application.SMTP;
using Domain.Entities.Comments;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using Domain.Entities.Video;
using FluentAssertions;
using Xunit;

namespace UnitTests.DomainAndModels;

public class DomainAndModelsTests
{
    [Fact]
    public void Models_CanBeAssigned()
    {
        var videoItem = new VideoItemModel { LikesCount = 3, DislikesCount = 1, IsLiked = true, Portfolio = new PortfolioItemModel(), Privacy = new VideoPrivacyItemModel() };
        videoItem.UserRatingPercent.Should().Be(75);
        new VideoItemModel().UserRatingPercent.Should().Be(0);

        var search = new VideoSearchModel { Q = "q", Title = "t", PortfolioName = "p", CreateYearFrom = "2020", CreateYearTo = "2024", SortBy = "date", PortfolioId = 1, IsLiked = true };
        search.Q.Should().Be("q");

        var create = new VideoCreateModel { Title = "t", Slug = "s", Description = "d", PrivacyId = 1, PortfolioId = 2 };
        create.PortfolioId.Should().Be(2);

        var update = new VideoUpdateModel { Id = 1, Title = "t", Slug = "s", Description = "d", PrivacyId = 1, PortfolioId = 2 };
        update.Id.Should().Be(1);

        var seed = new VideoSeedModel { Title = "t", Slug = "s", Description = "d", PrivacySystemCode = "PUBLIC", ImagePath = "i", VideoFile = "v" };
        seed.VideoFile.Should().Be("v");

        var pCreate = new PortfolioCreateModel { Name = "n", NickName = "nn", Description = "d", Specialization = "s", Contacts = "c", Settings = "set" };
        pCreate.Contacts.Should().Be("c");

        var pUpdate = new PortfolioUpdateModel { Id = 1, Name = "n", NickName = "nn", Description = "d", Specialization = "s", Contacts = "c", Settings = "set" };
        pUpdate.Id.Should().Be(1);

        new PortfolioDeleteModel { Id = 8 }.Id.Should().Be(8);
        new PortfolioSubscriptionModel { PortfolioId = 8 }.PortfolioId.Should().Be(8);
        var pSearch = new PortfolioSearchModel { Page = 2, Q = "dev", IsSubscribed = true };
        pSearch.Q.Should().Be("dev");
        pSearch.IsSubscribed.Should().BeTrue();

        var avatarMock = new Moq.Mock<Microsoft.AspNetCore.Http.IFormFile>();
        var bannerMock = new Moq.Mock<Microsoft.AspNetCore.Http.IFormFile>();
        pCreate.AvatarImage = avatarMock.Object;
        pCreate.BannerImage = bannerMock.Object;
        pCreate.AvatarImage.Should().Be(avatarMock.Object);
        pCreate.BannerImage.Should().Be(bannerMock.Object);

        new PortfolioDeleteModel { Id = 8 }.Id.Should().Be(8);
        new PortfolioSubscriptionModel { PortfolioId = 8 }.PortfolioId.Should().Be(8);
        new PortfolioItemModel { IsSubscribed = true, SubscriberCount = 4 }.SubscriberCount.Should().Be(4);

        new AccountResetPasswordModel { Email = "e", Token = "t", NewPassword = "n" }.Token.Should().Be("t");
        new AccountValidateResetTokenModel { Email = "e", Token = "t" }.Email.Should().Be("e");
        new AccountForgotPasswordModel { Email = "e" }.Email.Should().Be("e");
        new AccountGoogleLoginRequestModel { Token = "tok" }.Token.Should().Be("tok");
        new UserDeleteModel { Id = 1 }.Id.Should().Be(1);
        new UserEditModel { Id = 1, Email = "e", FirstName = "f", LastName = "l", Roles = ["User"] }.Roles.Should().Contain("User");
        new UserItemModel { Email = "e", FirstName = "f", LastName = "l", Image = "i", IsLoginGoogle = true, IsLoginPassword = true }.IsLoginGoogle.Should().BeTrue();
        new UserSearchModel().Page.Should().Be(1);
        new CommentsItemModal { Id = 1 }.Id.Should().Be(1);
        new PaginationModel { TotalCount = 1, TotalPages = 1, ItemsPerPage = 10, CurrentPage = 1 }.TotalCount.Should().Be(1);
        new VideoProgressUpdate { Percentage = 1, Status = "s", EstimatedTimeRemaining = "t" }.Status.Should().Be("s");
        new EmailMessage { Subject = "s", Body = "b", To = "t" }.To.Should().Be("t");
        new CreatePortfolioCommand(pCreate).Model.Should().Be(pCreate);
        new UpdateCommentRequest("x").Content.Should().Be("x");
    }

    [Fact]
    public void DomainEntities_CanBeAssigned()
    {
        var portfolio = new PortfolioEntity { Id = 1, Name = "P" };
        var subscriber = new PortfolioSubscriberEntity { PortfolioId = 1, UserId = 2 };
        var user = new UserEntity
        {
            Id = 5,
            FirstName = "A",
            LastName = "B",
            Image = "img.png",
            IsDeleted = true,
            DateCreated = DateTime.UtcNow,
            Portfolio = portfolio,
            SubscribedPortfolios = new List<PortfolioSubscriberEntity> { subscriber }
        };
        user.Portfolio.Should().Be(portfolio);
        user.SubscribedPortfolios.Should().Contain(subscriber);
        user.IsDeleted.Should().BeTrue();

        var reaction = new VideoReactionEntity { VideoId = 1, UserId = 2, IsLike = true, Video = new VideoEntity(), User = user };
        reaction.IsLike.Should().BeTrue();

        var sub = new PortfolioSubscriberEntity { PortfolioId = 1, UserId = 2, Portfolio = portfolio, User = user };
        sub.UserId.Should().Be(2);

        var role = new RoleEntity("Admin");
        role.Name.Should().Be("Admin");
        new RoleEntity();

        var userRole = new UserRoleEntity { User = user, Role = role };
        userRole.Role.Should().Be(role);

        var login = new UserLoginEntity { User = user };
        login.User.Should().NotBeNull();

        var comment = new CommentsEntity { Content = "c", LikesCount = 1, DislikeCount = 2, RepliesCount = 3, IsEdited = true };
        comment.RepliesCount.Should().Be(3);

        var privacy = new VideoPrivacyEntity { Name = "n", SystemCode = "c" };
        privacy.Videos.Should().NotBeNull();
    }

    [Fact]
    public void UnFreelancerizedAccessException_HasAllConstructors()
    {
        new UnFreelancerizedAccessException().Message.Should().Be("Доступ заборонено");
        new UnFreelancerizedAccessException("x").Message.Should().Be("x");
        var inner = new Exception("inner");
        var ex = new UnFreelancerizedAccessException("x", inner);
        ex.InnerException.Should().Be(inner);
    }
}
