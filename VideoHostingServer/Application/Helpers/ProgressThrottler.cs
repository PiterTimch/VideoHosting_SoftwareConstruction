using Application.Models.VideoProcessing;

namespace Application.Helpers;

public sealed class ProgressThrottler(Action<VideoProgressUpdate> onProgress, TimeSpan interval, double minPercentDelta = 1)
{
    private readonly object _lock = new();
    private DateTime _lastSentUtc = DateTime.MinValue;
    private double _lastPercent = -1;

    public void Report(VideoProgressUpdate update, bool force = false)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var percentDelta = Math.Abs(update.Percentage - _lastPercent);
            var intervalPassed = now - _lastSentUtc >= interval;
            var significantChange = percentDelta >= minPercentDelta;
            var isTerminal = update.Percentage >= 100
                || update.Status.StartsWith("Помилка", StringComparison.Ordinal)
                || update.Status is "Завершено" or "Completed";

            if (!force && !isTerminal && !intervalPassed && !significantChange)
            {
                return;
            }

            _lastSentUtc = now;
            _lastPercent = update.Percentage;
            onProgress(update);
        }
    }
}
