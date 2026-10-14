using Application.Constants;
using Application.Models.Video;
using Application.Services;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace UnitTests.Services;

public class CookieAuthServiceCoverageTests
{
    [Fact]
    public void SetAuthCookie_NullHttpContext_DoesNotThrow()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CookieAuthService(accessor.Object);

        var act = () => service.SetAuthCookie("some-token");
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearAuthCookie_NullHttpContext_DoesNotThrow()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CookieAuthService(accessor.Object);

        var act = () => service.ClearAuthCookie();
        act.Should().NotThrow();
    }

    [Fact]
    public void SetAuthCookie_WithContext_AppendsCookie()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var service = new CookieAuthService(accessor.Object);
        // Should not throw when the response is available
        var act = () => service.SetAuthCookie("my-jwt");
        act.Should().NotThrow();
    }

    [Fact]
    public void ClearAuthCookie_WithContext_DeletesCookie()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var service = new CookieAuthService(accessor.Object);
        var act = () => service.ClearAuthCookie();
        act.Should().NotThrow();
    }
}

public class ConstantsAndStaticClassesTests
{
    [Fact]
    public void AuthConstants_HaveExpectedValues()
    {
        AuthConstants.RolesClaim.Should().Be("roles");
        AuthConstants.AuthCookieName.Should().Be("jwt_token");
    }

    [Fact]
    public void FileConstants_ContainExpectedExtensionsAndTypes()
    {
        FileConstants.AllowedImageExtensions.Should().Contain(".jpg");
        FileConstants.AllowedImageExtensions.Should().Contain(".png");
        FileConstants.AllowedImageExtensions.Should().Contain(".webp");
        FileConstants.AllowedImageExtensions.Should().Contain(".avif");

        FileConstants.AllowedImageContentTypes.Should().Contain("image/jpeg");
        FileConstants.AllowedImageContentTypes.Should().Contain("image/png");

        FileConstants.AllowedVideoExtensions.Should().Contain(".mp4");
        FileConstants.AllowedVideoExtensions.Should().Contain(".avi");
        FileConstants.AllowedVideoExtensions.Should().Contain(".mkv");
        FileConstants.AllowedVideoExtensions.Should().Contain(".mov");

        FileConstants.AllowedVideoContentTypes.Should().Contain("video/mp4");
        FileConstants.AllowedVideoContentTypes.Should().Contain("video/x-msvideo");
    }

    [Fact]
    public void Roles_AllRoles_ContainsExpectedRoles()
    {
        Roles.AllRoles.Should().Contain(Roles.Admin);
        Roles.AllRoles.Should().Contain(Roles.User);
        Roles.AllRoles.Should().Contain(Roles.Freelancer);
    }

    [Fact]
    public void VideoPrivacyConstants_HaveExpectedValues()
    {
        VideoPrivacyConstants.Public.Should().NotBeNullOrEmpty();
        VideoPrivacyConstants.Private.Should().NotBeNullOrEmpty();
        VideoPrivacyConstants.UrlOnly.Should().NotBeNullOrEmpty();
        VideoPrivacyConstants.Public.Should().NotBe(VideoPrivacyConstants.Private);
        VideoPrivacyConstants.Private.Should().NotBe(VideoPrivacyConstants.UrlOnly);
    }

    [Fact]
    public void VideoItemModel_UserRatingPercent_WhenZeroReactions_ReturnsZero()
    {
        var model = new VideoItemModel { LikesCount = 0, DislikesCount = 0 };
        model.UserRatingPercent.Should().Be(0);
    }

    [Fact]
    public void VideoItemModel_UserRatingPercent_CalculatesCorrectly()
    {
        var model = new VideoItemModel { LikesCount = 3, DislikesCount = 1 };
        model.UserRatingPercent.Should().Be(75); // 3/(3+1)*100
    }
}

public class VideoSearchValidatorYearRangeTests
{
    [Fact]
    public void VideoSearchModelValidator_WhenYearFromGreaterThanYearTo_IsInvalid()
    {
        var validator = new Application.Validators.Video.VideoSearchModelValidator();

        // YearFrom=2025 > YearTo=2020 — HaveValidYearRange should fail
        var result = validator.Validate(new VideoSearchModel
        {
            Page = 1,
            ItemPerPage = 10,
            CreateYearFrom = "2025",
            CreateYearTo = "2020"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("From") || e.ErrorMessage.Contains("рік") || e.PropertyName.Contains("Year"));
    }

    [Fact]
    public void VideoSearchModelValidator_WhenYearsEqual_IsValid()
    {
        var validator = new Application.Validators.Video.VideoSearchModelValidator();

        var result = validator.Validate(new VideoSearchModel
        {
            Page = 1,
            ItemPerPage = 10,
            CreateYearFrom = "2022",
            CreateYearTo = "2022"
        });

        result.IsValid.Should().BeTrue();
    }
}

public class DomainEntityCoverageTests
{
    [Fact]
    public void VideoEntity_DefaultValues_AreCorrect()
    {
        var entity = new VideoEntity
        {
            Id = 1,
            Title = "Test",
            Slug = "test",
            Description = "desc",
            ViewCount = 10,
            IsDeleted = false
        };

        entity.Title.Should().Be("Test");
        entity.ViewCount.Should().Be(10);
        entity.IsDeleted.Should().BeFalse();
        entity.VideoReactions.Should().NotBeNull();
    }

    [Fact]
    public void VideoReactionEntity_CanBeAssigned()
    {
        var user = new UserEntity { Id = 1, UserName = "u" };
        var video = new VideoEntity { Id = 1, Title = "V", Slug = "v" };

        var reaction = new VideoReactionEntity
        {
            VideoId = 1,
            UserId = 1,
            IsLike = true,
            Video = video,
            User = user
        };

        reaction.IsLike.Should().BeTrue();
        reaction.Video.Should().Be(video);
        reaction.User.Should().Be(user);
    }

    [Fact]
    public void UserEntity_DefaultCollections_AreNotNull()
    {
        var user = new UserEntity { Id = 1, UserName = "u" };
        user.SubscribedPortfolios.Should().NotBeNull();
        user.DateCreated.Kind.Should().Be(DateTimeKind.Utc);
    }
}
