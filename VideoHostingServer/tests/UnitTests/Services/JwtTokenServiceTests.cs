using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces;
using Application.Services;
using Domain.Entities.Identity;
using Domain.Entities.Portfolio;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly Mock<IGenericRepository<PortfolioEntity, long>> _portfolioRepoMock;

    public JwtTokenServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        var store = new Mock<IUserStore<UserEntity>>();
        _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
        _portfolioRepoMock = new Mock<IGenericRepository<PortfolioEntity, long>>();
    }

    [Fact]
    public async Task CreateTokenAsync_WithoutPortfolio_ReturnsValidJwt()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsAVeryLongSecretKeyForTestingJwtTokenGeneration1234567890!");
        var user = new UserEntity
        {
            Id = 1,
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe",
            Image = "john.jpg"
        };

        var portfolios = new List<PortfolioEntity>().BuildMockDbSet();
        _portfolioRepoMock.Setup(r => r.AsQurable(false)).Returns(portfolios.Object);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var service = new JwtTokenService(_configMock.Object, _userManagerMock.Object, _portfolioRepoMock.Object);

        var token = await service.CreateTokenAsync(user);

        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "john@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "name" && c.Value == "Doe John");
        jwt.Claims.Should().NotContain(c => c.Type == "portfolioId");
    }

    [Fact]
    public async Task CreateTokenAsync_WithPortfolio_IncludesPortfolioIdClaim()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);
        var user = new UserEntity
        {
            Id = 2,
            Email = "freelancer@test.com",
            FirstName = "Jane",
            LastName = "Smith"
        };

        var portfolio = new PortfolioEntity { Id = 99, Freelancer = user };
        var portfolios = new List<PortfolioEntity> { portfolio }.BuildMockDbSet();
        _portfolioRepoMock.Setup(r => r.AsQurable(false)).Returns(portfolios.Object);
        _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Freelancer", "User" });

        var service = new JwtTokenService(_configMock.Object, _userManagerMock.Object, _portfolioRepoMock.Object);

        var token = await service.CreateTokenAsync(user);

        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "portfolioId" && c.Value == "99");
    }
}
