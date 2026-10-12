using Application.Constants;
using Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class CookieAuthServiceTests
{
    [Fact]
    public void SetAuthCookie_WithValidHttpContext_AppendsAuthCookie()
    {
        var context = new DefaultHttpContext();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(context);
        var service = new CookieAuthService(mockAccessor.Object);

        service.SetAuthCookie("test_jwt_123");

        var headers = context.Response.Headers["Set-Cookie"].ToString();
        headers.Should().Contain(AuthConstants.AuthCookieName + "=test_jwt_123");
        headers.Should().Contain("httponly");
        headers.Should().Contain("secure");
        headers.Should().Contain("path=/");
    }

    [Fact]
    public void SetAuthCookie_WhenHttpContextIsNull_DoesNotThrow()
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CookieAuthService(mockAccessor.Object);

        var action = () => service.SetAuthCookie("token");

        action.Should().NotThrow();
    }

    [Fact]
    public void ClearAuthCookie_WithValidHttpContext_SetsExpiredCookie()
    {
        var context = new DefaultHttpContext();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(context);
        var service = new CookieAuthService(mockAccessor.Object);

        service.ClearAuthCookie();

        var headers = context.Response.Headers["Set-Cookie"].ToString();
        headers.Should().Contain(AuthConstants.AuthCookieName + "=;");
        headers.Should().Contain("expires=");
    }

    [Fact]
    public void ClearAuthCookie_WhenHttpContextIsNull_DoesNotThrow()
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CookieAuthService(mockAccessor.Object);

        var action = () => service.ClearAuthCookie();

        action.Should().NotThrow();
    }
}
