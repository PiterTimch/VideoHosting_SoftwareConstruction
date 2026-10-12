using Application.Helpers;
using Application.Models.VideoProcessing;
using FluentAssertions;
using Xunit;

namespace UnitTests.HelpersAndJobs;

public class ProgressThrottlerTests
{
    [Fact]
    public void Report_ForceTrue_AlwaysCallsCallback()
    {
        int callCount = 0;
        var throttler = new ProgressThrottler(_ => callCount++, TimeSpan.FromMinutes(10), minPercentDelta: 50);

        throttler.Report(new VideoProgressUpdate { Percentage = 5, Status = "Encoding" }, force: true);
        throttler.Report(new VideoProgressUpdate { Percentage = 6, Status = "Encoding" }, force: true);

        callCount.Should().Be(2);
    }

    [Fact]
    public void Report_TerminalStatus_BypassesThrottling()
    {
        var reported = new List<string>();
        var throttler = new ProgressThrottler(u => reported.Add(u.Status), TimeSpan.FromMinutes(10), minPercentDelta: 50);

        throttler.Report(new VideoProgressUpdate { Percentage = 1, Status = "Initial" }, force: true);
        throttler.Report(new VideoProgressUpdate { Percentage = 100, Status = "Done" }, force: false);
        throttler.Report(new VideoProgressUpdate { Percentage = 1, Status = "Помилка: File corrupt" }, force: false);
        throttler.Report(new VideoProgressUpdate { Percentage = 1, Status = "Завершено" }, force: false);
        throttler.Report(new VideoProgressUpdate { Percentage = 1, Status = "Completed" }, force: false);

        reported.Should().Contain(["Initial", "Done", "Помилка: File corrupt", "Завершено", "Completed"]);
    }

    [Fact]
    public void Report_BelowDeltaAndInterval_IsThrottled()
    {
        int callCount = 0;
        var throttler = new ProgressThrottler(_ => callCount++, TimeSpan.FromHours(1), minPercentDelta: 5);

        throttler.Report(new VideoProgressUpdate { Percentage = 2, Status = "Encoding" }, force: false);
        callCount.Should().Be(1);

        throttler.Report(new VideoProgressUpdate { Percentage = 3, Status = "Encoding" }, force: false);
        callCount.Should().Be(1);

        throttler.Report(new VideoProgressUpdate { Percentage = 9, Status = "Encoding" }, force: false);
        callCount.Should().Be(2);
    }
}
