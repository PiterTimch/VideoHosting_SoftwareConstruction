using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace IntegrationTests
{
    public class SystemIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SystemIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_VideosEndpoint_ReturnsExpectedResult()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Videos/GetVideos");
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }
        
        [Fact]
        public async Task Get_UnknownEndpoint_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/UnknownEndpoint123");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void DI_JwtTokenService_IsRegistered()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<IJwtTokenService>();
            service.Should().NotBeNull();
        }
        
        [Fact]
        public void DI_SmtpService_IsRegistered()
        {
            using var scope = _factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<ISmtpService>();
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task DB_CanConnect_Successfully()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            bool canConnect = await dbContext.Database.CanConnectAsync();
            canConnect.Should().BeTrue();
        }
        
        [Fact]
        public void DB_DbSet_Users_Exists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Users.Should().NotBeNull();
        }
    }
}
