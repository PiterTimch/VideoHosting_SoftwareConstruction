using System.Collections.Concurrent;
using Application.Interfaces;
using Application.Models.VideoProcessing;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class VideoProgressStore(ILogger<VideoProgressStore> logger) : IVideoProgressStore
{
    private readonly ConcurrentDictionary<string, VideoProgressUpdate> _progress = new();
    private readonly ConcurrentDictionary<string, (double Percentage, string Status, DateTime LastLogUtc)> _lastLog = new();
    private static readonly TimeSpan LogInterval = TimeSpan.FromSeconds(3);

    public void Set(string trackingId, VideoProgressUpdate update)
    {
        _progress[trackingId] = update;

        var now = DateTime.UtcNow;
        var shouldLog = !_lastLog.TryGetValue(trackingId, out var prev)
            || now - prev.LastLogUtc >= LogInterval
            || Math.Abs(prev.Percentage - update.Percentage) >= 2
            || prev.Status != update.Status
            || update.Percentage >= 100;

        if (shouldLog)
        {
            _lastLog[trackingId] = (update.Percentage, update.Status, now);
            logger.LogInformation(
                "[VideoProgress] Store SET trackingId={TrackingId} status={Status} percentage={Percentage}",
                trackingId,
                update.Status,
                update.Percentage);
        }
    }

    public VideoProgressUpdate? Get(string trackingId) =>
        _progress.TryGetValue(trackingId, out var update) ? update : null;
}
