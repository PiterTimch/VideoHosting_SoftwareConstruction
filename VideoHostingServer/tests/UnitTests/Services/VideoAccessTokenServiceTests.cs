using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace UnitTests.Services;

public class VideoAccessTokenServiceTests
{
    private static VideoAccessTokenService CreateService(string? key = null)
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns(key ?? "TestSecretKeyForVideoAccessTokenServiceUnit1234567890!");
        return new VideoAccessTokenService(configMock.Object);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var service = CreateService();
        var token = service.GenerateToken(42, TimeSpan.FromMinutes(10));
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        var service = CreateService();
        var token = service.GenerateToken(7, TimeSpan.FromMinutes(5));
        service.ValidateToken(token, 7).Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WrongVideoId_ReturnsFalse()
    {
        var service = CreateService();
        var token = service.GenerateToken(1, TimeSpan.FromMinutes(5));
        service.ValidateToken(token, 99).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ExpiredToken_ReturnsFalse()
    {
        var service = CreateService();
        var token = service.GenerateToken(5, TimeSpan.FromSeconds(-1));
        service.ValidateToken(token, 5).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsFalse()
    {
        var service = CreateService();
        var token = service.GenerateToken(10, TimeSpan.FromMinutes(5));
        var tampered = token[..^2] + "XX";
        service.ValidateToken(tampered, 10).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsFalse()
    {
        var service = CreateService();
        service.ValidateToken(string.Empty, 1).Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_RandomString_ReturnsFalse()
    {
        var service = CreateService();
        service.ValidateToken("not-a-real-token", 1).Should().BeFalse();
    }

    [Fact]
    public void GenerateToken_DifferentKeys_ProduceDifferentTokens()
    {
        var service1 = CreateService("KeyAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        var service2 = CreateService("KeyBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=");
        var t1 = service1.GenerateToken(1, TimeSpan.FromMinutes(10));
        var t2 = service2.GenerateToken(1, TimeSpan.FromMinutes(10));
        t1.Should().NotBe(t2);
    }

    [Fact]
    public void ValidateToken_TokenFromDifferentKey_ReturnsFalse()
    {
        var service1 = CreateService("KeyAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        var service2 = CreateService("KeyBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=");
        var token = service1.GenerateToken(1, TimeSpan.FromMinutes(10));
        service2.ValidateToken(token, 1).Should().BeFalse();
    }
}
