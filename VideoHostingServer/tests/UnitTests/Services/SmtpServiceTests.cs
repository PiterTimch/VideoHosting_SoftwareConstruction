using Application.Services;
using Application.SMTP;
using FluentAssertions;
using Xunit;

namespace UnitTests.Services;

public class SmtpServiceTests
{
    [Fact]
    public void CanInstantiate()
    {
        var service = new SmtpService();
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_ExecutesSendSafely()
    {
        var service = new SmtpService();
        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "<p>Hello</p>"
        };

        var result = await service.SendEmailAsync(message);

        result.Should().BeTrue();
    }
}
