using Application.Models.VideoProcessing;
using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class VideoProgressStoreTests
{
    private readonly Mock<ILogger<VideoProgressStore>> _loggerMock;
    private readonly VideoProgressStore _store;

    public VideoProgressStoreTests()
    {
        _loggerMock = new Mock<ILogger<VideoProgressStore>>();
        _store = new VideoProgressStore(_loggerMock.Object);
    }

    [Fact]
    public void SetAndGet_SavesAndRetrievesProgressUpdate()
    {
        var update = new VideoProgressUpdate
        {
            Percentage = 15.5,
            Status = "Encoding 720p",
            EstimatedTimeRemaining = "01:30"
        };

        _store.Set("tid-1", update);

        var retrieved = _store.Get("tid-1");
        retrieved.Should().NotBeNull();
        retrieved!.Percentage.Should().Be(15.5);
        retrieved.Status.Should().Be("Encoding 720p");
        retrieved.EstimatedTimeRemaining.Should().Be("01:30");
    }

    [Fact]
    public void Get_WhenNotFound_ReturnsNull()
    {
        var result = _store.Get("non-existing-id");
        result.Should().BeNull();
    }

    [Fact]
    public void Set_MultipleUpdates_CoversAllLoggingBranches()
    {
        _store.Set("tid-branches", new VideoProgressUpdate { Percentage = 10, Status = "Queue" });
        _store.Set("tid-branches", new VideoProgressUpdate { Percentage = 11, Status = "Queue" });
        _store.Set("tid-branches", new VideoProgressUpdate { Percentage = 14, Status = "Queue" });
        _store.Set("tid-branches", new VideoProgressUpdate { Percentage = 14, Status = "Encoding" });
        _store.Set("tid-branches", new VideoProgressUpdate { Percentage = 100, Status = "Done" });

        var finalResult = _store.Get("tid-branches");
        finalResult.Should().NotBeNull();
        finalResult!.Percentage.Should().Be(100);
    }
}
