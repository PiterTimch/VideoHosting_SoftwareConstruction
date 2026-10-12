using System.Security.Claims;
using Application.Constants;
using Application.Exceptions;
using Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
    private readonly CurrentUserService _service;

    public CurrentUserServiceTests()
    {
        _contextAccessorMock = new Mock<IHttpContextAccessor>();
        _service = new CurrentUserService(_contextAccessorMock.Object);
    }

    [Fact]
    public void GetCurrentUserId_WhenUserAuthenticated_ReturnsUserId()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "42") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var id = _service.GetCurrentUserId();

        id.Should().Be(42);
    }

    [Fact]
    public void GetCurrentUserId_WhenNoUser_ThrowsUnFreelancerizedAccessException()
    {
        _contextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var action = () => _service.GetCurrentUserId();

        action.Should().Throw<UnFreelancerizedAccessException>();
    }

    [Fact]
    public void TryGetCurrentUserId_WhenHttpContextIsNull_ReturnsNull()
    {
        _contextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = _service.TryGetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetCurrentUserId_WhenClaimMissing_ReturnsNull()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var result = _service.TryGetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetCurrentUserId_WhenClaimNotLong_ReturnsNull()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "invalid_number") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var result = _service.TryGetCurrentUserId();

        result.Should().BeNull();
    }

    [Fact]
    public void IsInRole_WhenHttpContextNull_ReturnsFalse()
    {
        _contextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = _service.IsInRole("Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_WhenStandardRoleClaimPresent_ReturnsTrue()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var result = _service.IsInRole("Admin");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_WhenRolesClaimIsJsonArray_MatchesRole()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.RolesClaim, "[\"Admin\",\"Moderator\"]") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.IsInRole("Admin").Should().BeTrue();
        _service.IsInRole("Moderator").Should().BeTrue();
        _service.IsInRole("Guest").Should().BeFalse();
    }

    [Fact]
    public void IsInRole_WhenRolesClaimIsSingleString_MatchesRole()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(AuthConstants.RolesClaim, "Moderator") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.IsInRole("Moderator").Should().BeTrue();
        _service.IsInRole("Admin").Should().BeFalse();
    }

    [Fact]
    public void IsInRole_WhenNoRolesClaim_ReturnsFalse()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        var result = _service.IsInRole("Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_WhenCustomRoleClaimType_MatchesViaFindAll()
    {
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity("TestAuth", ClaimTypes.Name, "CustomRoleClaimType");
        identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "moderator"));
        context.User = new ClaimsPrincipal(identity);
        _contextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _service.IsInRole("Admin").Should().BeTrue();
        _service.IsInRole("SuperAdmin").Should().BeFalse();
    }
}
