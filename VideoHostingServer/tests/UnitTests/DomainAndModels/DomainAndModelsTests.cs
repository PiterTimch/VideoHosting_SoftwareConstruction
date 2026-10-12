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
        new PortfolioSearchModel { Page = 2 }.Page.Should().Be(2);
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
        var reaction = new VideoReactionEntity { VideoId = 1, UserId = 2, IsLike = true, Video = new VideoEntity(), User = new UserEntity() };
        reaction.IsLike.Should().BeTrue();

        var sub = new PortfolioSubscriberEntity { PortfolioId = 1, UserId = 2, Portfolio = new PortfolioEntity(), User = new UserEntity() };
        sub.UserId.Should().Be(2);

        var role = new RoleEntity("Admin");
        role.Name.Should().Be("Admin");
        new RoleEntity();

        var userRole = new UserRoleEntity { User = new UserEntity(), Role = role };
        userRole.Role.Should().Be(role);

        var login = new UserLoginEntity { User = new UserEntity() };
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
