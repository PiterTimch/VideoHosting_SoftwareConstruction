using Application.Helpers;
using Application.Models.VideoProcessing;
using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class ProgressThrottlerTests
{
    [Fact]
    public void Report_FirstCall_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(10));

        throttler.Report(new VideoProgressUpdate { Percentage = 10, Status = "Start" });

        calls.Should().HaveCount(1);
        calls[0].Percentage.Should().Be(10);
    }

    [Fact]
    public void Report_SmallDeltaWithinInterval_Suppressed()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60), minPercentDelta: 5);

        throttler.Report(new VideoProgressUpdate { Percentage = 10, Status = "Encoding" });
        // second call: only 1% change within 60s — should be suppressed
        throttler.Report(new VideoProgressUpdate { Percentage = 11, Status = "Encoding" });

        calls.Should().HaveCount(1);
    }

    [Fact]
    public void Report_SignificantPercentDelta_Fires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60), minPercentDelta: 5);

        throttler.Report(new VideoProgressUpdate { Percentage = 10, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 20, Status = "Encoding" }); // +10 >= 5

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_TerminalStatus_100Percent_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60));

        throttler.Report(new VideoProgressUpdate { Percentage = 50, Status = "Encoding" });
        // 100% is terminal — fires even within interval
        throttler.Report(new VideoProgressUpdate { Percentage = 100, Status = "Encoding" });

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_TerminalStatusCompleted_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60));

        throttler.Report(new VideoProgressUpdate { Percentage = 50, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 55, Status = "Завершено" });

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_TerminalStatusError_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60));

        throttler.Report(new VideoProgressUpdate { Percentage = 50, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 51, Status = "Помилка: ffmpeg failed" });

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_TerminalStatusEnglishCompleted_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60));

        throttler.Report(new VideoProgressUpdate { Percentage = 30, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 31, Status = "Completed" });

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_Force_AlwaysFires()
    {
        var calls = new List<VideoProgressUpdate>();
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.FromSeconds(60));

        throttler.Report(new VideoProgressUpdate { Percentage = 10, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 10.5, Status = "Encoding" }, force: true);

        calls.Should().HaveCount(2);
    }

    [Fact]
    public void Report_IntervalExpired_Fires()
    {
        var calls = new List<VideoProgressUpdate>();
        // Use a zero-length interval so it always expires immediately
        var throttler = new ProgressThrottler(calls.Add, TimeSpan.Zero);

        throttler.Report(new VideoProgressUpdate { Percentage = 10, Status = "Encoding" });
        throttler.Report(new VideoProgressUpdate { Percentage = 10.1, Status = "Encoding" }); // interval expired

        calls.Should().HaveCount(2);
    }
}

public class VideoProgressStoreThrottlingTests
{
    [Fact]
    public void Set_StatusChange_LogsImmediately()
    {
        var logger = new Mock<ILogger<VideoProgressStore>>();
        var store = new VideoProgressStore(logger.Object);

        store.Set("t1", new VideoProgressUpdate { Percentage = 50, Status = "Encoding 720p" });
        store.Set("t1", new VideoProgressUpdate { Percentage = 50, Status = "Encoding 1080p" }); // same% diff status

        var retrieved = store.Get("t1");
        retrieved!.Status.Should().Be("Encoding 1080p");
    }

    [Fact]
    public void Set_LargePercentJump_LogsImmediately()
    {
        var logger = new Mock<ILogger<VideoProgressStore>>();
        var store = new VideoProgressStore(logger.Object);

        store.Set("t2", new VideoProgressUpdate { Percentage = 10, Status = "Encoding" });
        // jump by 3 >= 2 triggers log
        store.Set("t2", new VideoProgressUpdate { Percentage = 13, Status = "Encoding" });

        store.Get("t2")!.Percentage.Should().Be(13);
    }
}
