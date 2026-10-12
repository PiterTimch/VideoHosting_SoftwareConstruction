using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace UnitTests.Services;

public class VideoFileServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly IConfiguration _config;

    public VideoFileServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "VideoFileServiceTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"MediaRoot", _testDir},
            {"VideosDir", "videos"},
            {"VideoSizes:0", "720"},
            {"VideoSizes:1", "1080"},
            {"VideoProcessing:FfmpegThreads", "1"},
            {"VideoProcessing:ProgressUpdateIntervalSeconds", "1"},
            {"VideoProcessing:MinProgressPercentDelta", "1"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    [Fact]
    public async Task DeleteVideoAsync_DeletesAllConfiguredSizeFiles()
    {
        var service = new VideoFileService(_config);
        var targetVideosDir = Path.Combine(_testDir, "videos");
        Directory.CreateDirectory(targetVideosDir);

        var file720 = Path.Combine(targetVideosDir, "720_myvideo.mp4");
        var file1080 = Path.Combine(targetVideosDir, "1080_myvideo.mp4");

        await File.WriteAllTextAsync(file720, "dummy");
        await File.WriteAllTextAsync(file1080, "dummy");

        File.Exists(file720).Should().BeTrue();
        File.Exists(file1080).Should().BeTrue();

        await service.DeleteVideoAsync("myvideo.mp4");

        File.Exists(file720).Should().BeFalse();
        File.Exists(file1080).Should().BeFalse();
    }
}
