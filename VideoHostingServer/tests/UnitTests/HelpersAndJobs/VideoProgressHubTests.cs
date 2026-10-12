using Application.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.HelpersAndJobs;

public class VideoProgressHubTests
{
    private readonly Mock<ILogger<VideoProgressHub>> _loggerMock = new();
    private readonly Mock<HubCallerContext> _contextMock = new();
    private readonly Mock<IGroupManager> _groupsMock = new();
    private readonly VideoProgressHub _hub;

    public VideoProgressHubTests()
    {
        _hub = new VideoProgressHub(_loggerMock.Object)
        {
            Context = _contextMock.Object,
            Groups = _groupsMock.Object
        };
        _contextMock.Setup(c => c.ConnectionId).Returns("conn_123");
    }

    [Fact]
    public async Task JoinPortfolio_AddsConnectionToGroup()
    {
        await _hub.JoinPortfolio("track_abc");

        _groupsMock.Verify(g => g.AddToGroupAsync("conn_123", "track_abc", default), Times.Once);
    }

    [Fact]
    public async Task LeavePortfolio_RemovesConnectionFromGroup()
    {
        await _hub.LeavePortfolio("track_abc");

        _groupsMock.Verify(g => g.RemoveFromGroupAsync("conn_123", "track_abc", default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAndDisconnected_ExecuteWithoutErrors()
    {
        await _hub.OnConnectedAsync();

        await _hub.OnDisconnectedAsync(new Exception("test disconnect"));
    }
}
