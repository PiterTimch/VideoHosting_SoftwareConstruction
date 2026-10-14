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

        await service.DeleteVideoAsync("myvideo.mp4");

        File.Exists(file720).Should().BeFalse();
        File.Exists(file1080).Should().BeFalse();
    }

    private static string GetSampleVideoPath()
    {
        var local = Path.Combine(AppContext.BaseDirectory, "sample_video.mp4");
        if (File.Exists(local)) return local;
        var alt = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\..\VideoHostingWeb\tests\fixtures\sample_video.mp4"));
        if (File.Exists(alt)) return alt;
        throw new FileNotFoundException("Sample video not found");
    }

    [Fact]
    public async Task SaveVideoWithProgressAsync_EncodesVideoAndReportsProgress()
    {
        var samplePath = GetSampleVideoPath();
        var progressUpdates = new List<Application.Models.VideoProcessing.VideoProgressUpdate>();

        var quickConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MediaRoot"] = _testDir,
                ["VideosDir"] = "videos",
                ["VideoSizes:0"] = "240",
                ["VideoProcessing:FfmpegThreads"] = "2",
                ["VideoProcessing:ProgressUpdateIntervalSeconds"] = "0",
                ["VideoProcessing:MinProgressPercentDelta"] = "0"
            })
            .Build();

        var service = new VideoFileService(quickConfig);
        var baseFileName = await service.SaveVideoWithProgressAsync(samplePath, p => progressUpdates.Add(p));

        baseFileName.Should().EndWith(".mp4");
        var output240 = Path.Combine(_testDir, "videos", $"240_{baseFileName}");
        File.Exists(output240).Should().BeTrue();

        progressUpdates.Should().NotBeEmpty();
        progressUpdates.Last().Percentage.Should().Be(100);

        await service.DeleteVideoAsync(baseFileName);
    }

    [Fact]
    public async Task SaveVideoAsync_WithFormFile_SavesVideoAndCleansTempFile()
    {
        var samplePath = GetSampleVideoPath();
        await using var stream = File.OpenRead(samplePath);
        var formFile = new Microsoft.AspNetCore.Http.FormFile(stream, 0, stream.Length, "video", "sample.mp4");

        var quickConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MediaRoot"] = _testDir,
                ["VideosDir"] = "videos",
                ["VideoSizes:0"] = "240",
                ["VideoProcessing:FfmpegThreads"] = "2",
                ["VideoProcessing:ProgressUpdateIntervalSeconds"] = "0",
                ["VideoProcessing:MinProgressPercentDelta"] = "0"
            })
            .Build();

        var service = new VideoFileService(quickConfig);
        var baseFileName = await service.SaveVideoAsync(formFile);

        baseFileName.Should().EndWith(".mp4");
        var output240 = Path.Combine(_testDir, "videos", $"240_{baseFileName}");
        File.Exists(output240).Should().BeTrue();

        await service.DeleteVideoAsync(baseFileName);
    }

    [Fact]
    public void VideoFileService_FallbackMediaRoot_InitializesSuccessfully()
    {
        var targetVideosDir = Path.Combine(Path.GetTempPath(), "FallbackVideos_" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MediaRoot"] = "",
                    ["VideosDir"] = Path.GetRelativePath(Directory.GetCurrentDirectory(), targetVideosDir),
                    ["VideoSizes:0"] = "240"
                })
                .Build();

            var service = new VideoFileService(config);
            service.Should().NotBeNull();
            Directory.Exists(targetVideosDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(targetVideosDir)) Directory.Delete(targetVideosDir, true);
        }
    }
}
