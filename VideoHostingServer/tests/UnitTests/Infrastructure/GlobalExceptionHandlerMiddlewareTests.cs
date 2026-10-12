using FluentAssertions;
using Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Infrastructure;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_WhenNextSucceeds_LeavesStatusUnchanged()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsException_Sets400AndWritesError()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = ctx => throw new InvalidOperationException("Something went wrong");
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        responseBody.Should().Contain("Something went wrong");
    }
}
