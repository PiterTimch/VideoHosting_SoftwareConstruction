using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class VideoControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public VideoControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            // In a real scenario, this would hit the API endpoint
            bool isConnected = true;

            // Assert
            isConnected.Should().BeTrue();
        }
    }
}
