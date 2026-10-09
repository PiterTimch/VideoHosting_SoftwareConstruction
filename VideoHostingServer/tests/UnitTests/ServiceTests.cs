using Application.Services;
using Domain.Entities.Identity;
using Domain.Entities.Video;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System.Threading.Tasks;

namespace UnitTests
{
    public class ServiceTests
    {
        // --- CookieAuthService Tests ---
        [Fact]
        public void CookieAuthService_SetAuthCookie_DoesNotThrow()
        {
            var context = new DefaultHttpContext();
            var mockContextAccessor = new Mock<IHttpContextAccessor>();
            mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
            var service = new CookieAuthService(mockContextAccessor.Object);

            service.SetAuthCookie("test_token");
            
            var headers = context.Response.Headers["Set-Cookie"];
            headers.Should().Contain(h => h.Contains("jwt=test_token"));
        }

        [Fact]
        public void CookieAuthService_ClearAuthCookie_RemovesCookie()
        {
            var context = new DefaultHttpContext();
            var mockContextAccessor = new Mock<IHttpContextAccessor>();
            mockContextAccessor.Setup(x => x.HttpContext).Returns(context);
            var service = new CookieAuthService(mockContextAccessor.Object);

            service.ClearAuthCookie();
            
            var headers = context.Response.Headers["Set-Cookie"];
            headers.Should().Contain(h => h.Contains("jwt=; expires"));
        }
        
        [Fact]
        public void CookieAuthService_NoHttpContext_DoesNothing()
        {
            var mockContextAccessor = new Mock<IHttpContextAccessor>();
            mockContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
            var service = new CookieAuthService(mockContextAccessor.Object);

            var exception = Record.Exception(() => service.SetAuthCookie("test_token"));
            exception.Should().BeNull();
        }

        // --- SmtpService Tests ---
        [Fact]
        public void SmtpService_CanInstantiate()
        {
            var service = new SmtpService();
            service.Should().NotBeNull();
        }

        // --- VideoRecommendationService Tests ---
        [Fact]
        public void VideoRecommendationService_ComputeScore_ExactMatch_ReturnsHigh()
        {
            var service = new VideoRecommendationService();
            var v1 = new VideoEntity { Title = "React tutorial", Description = "Learn React fast" };
            var v2 = new VideoEntity { Title = "React tutorial", Description = "Learn React fast" };

            int score = service.ComputeScore(v1, v2);
            
            // "react", "tutorial" = 2 * 3 = 6
            // "learn", "react", "fast" = 3 * 1 = 3
            // total = 9
            score.Should().Be(9);
        }
        
        [Fact]
        public void VideoRecommendationService_ComputeScore_NoMatch_ReturnsZero()
        {
            var service = new VideoRecommendationService();
            var v1 = new VideoEntity { Title = "React tutorial" };
            var v2 = new VideoEntity { Title = "Angular course" };

            int score = service.ComputeScore(v1, v2);
            score.Should().Be(0);
        }
        
        [Fact]
        public void VideoRecommendationService_ComputeScore_EmptyTitle_HandlesSafely()
        {
            var service = new VideoRecommendationService();
            var v1 = new VideoEntity { Title = "" };
            var v2 = new VideoEntity { Title = "Angular course" };

            int score = service.ComputeScore(v1, v2);
            score.Should().Be(0);
        }
    }
}
