using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Application.Hubs;

public class VideoProgressHub(ILogger<VideoProgressHub> logger) : Hub
{
    public async Task JoinPortfolio(string trackingId)
    {
        logger.LogInformation(
            "[VideoProgress] Hub JoinPortfolio connectionId={ConnectionId} trackingId={TrackingId}",
            Context.ConnectionId,
            trackingId);

        await Groups.AddToGroupAsync(Context.ConnectionId, trackingId);
    }

    public async Task LeavePortfolio(string trackingId)
    {
        logger.LogInformation(
            "[VideoProgress] Hub LeavePortfolio connectionId={ConnectionId} trackingId={TrackingId}",
            Context.ConnectionId,
            trackingId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, trackingId);
    }

    public override Task OnConnectedAsync()
    {
        logger.LogInformation(
            "[VideoProgress] Hub OnConnected connectionId={ConnectionId}",
            Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation(
            exception,
            "[VideoProgress] Hub OnDisconnected connectionId={ConnectionId}",
            Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
