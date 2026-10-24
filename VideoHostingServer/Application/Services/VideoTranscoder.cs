using Application.Helpers;
using Application.Models.VideoProcessing;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace Application.Services;

public class VideoTranscoder
{
    private readonly VideoFileStorage _storage;
    private readonly FfmpegRuntime _ffmpegRuntime;
    private readonly List<int> _videoSizes;
    private readonly int _ffmpegThreads;
    private readonly TimeSpan _progressUpdateInterval;
    private readonly double _minProgressPercentDelta;
    private readonly IVideoCodecStrategy _codecStrategy;

    public VideoTranscoder(IConfiguration configuration, VideoFileStorage storage)
    {
        _storage = storage;
        _ffmpegRuntime = new FfmpegRuntime();
        _videoSizes = configuration.GetSection("VideoSizes").Get<List<int>>()!;
        _ffmpegThreads = configuration.GetValue("VideoProcessing:FfmpegThreads", 2);
        _progressUpdateInterval = TimeSpan.FromSeconds(configuration.GetValue("VideoProcessing:ProgressUpdateIntervalSeconds", 3));
        _minProgressPercentDelta = configuration.GetValue("VideoProcessing:MinProgressPercentDelta", 2);
        _codecStrategy = VideoCodecStrategyFactory.Create(configuration["VideoProcessing:VideoCodec"]);
    }

    public Task<string> SaveVideoFromFilePathAsync(string filePath)
    {
        return SaveVideoWithProgressAsync(filePath, _ => { });
    }

    public async Task<string> SaveVideoWithProgressAsync(string filePath, Action<VideoProgressUpdate> onProgress)
    {
        var throttler = new ProgressThrottler(onProgress, _progressUpdateInterval, _minProgressPercentDelta);
        await _ffmpegRuntime.EnsureAvailableAsync(update => throttler.Report(update, force: true));
        var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
        var baseFileName = $"{Guid.NewGuid()}.mp4";
        var startTime = DateTime.UtcNow;
        var totalSizes = _videoSizes.Count;

        for (var index = 0; index < totalSizes; index++)
        {
            var size = _videoSizes[index];
            var completedSizes = index;

            await ProcessVideoAsync(filePath, baseFileName, size, mediaInfo, sizePercent =>
            {
                var totalProgress = ((completedSizes + sizePercent / 100.0) / totalSizes) * 100;
                var remainingTimeText = "Calculating...";
                if (totalProgress > 0)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    var totalEstimatedTime = TimeSpan.FromTicks((long)(elapsed.Ticks / (totalProgress / 100)));
                    var remaining = totalEstimatedTime - elapsed;
                    remainingTimeText = remaining.TotalHours >= 1
                        ? remaining.ToString(@"hh\:mm\:ss")
                        : remaining.ToString(@"mm\:ss");
                }

                throttler.Report(new VideoProgressUpdate
                {
                    Percentage = Math.Round(totalProgress, 2),
                    Status = $"Processing {size}p",
                    EstimatedTimeRemaining = remainingTimeText,
                });
            });
        }

        throttler.Report(new VideoProgressUpdate
        {
            Percentage = 100,
            Status = "Processing",
            EstimatedTimeRemaining = "00:00:00",
        }, force: true);

        return baseFileName;
    }

    private async Task ProcessVideoAsync(
        string inputPath,
        string baseName,
        int height,
        IMediaInfo mediaInfo,
        Action<double>? onProgress = null)
    {
        var outputPath = _storage.GetOutputPath(height, baseName);
        var conversion = VideoConversionDirector.Build(
            mediaInfo,
            height,
            _ffmpegThreads,
            outputPath,
            _codecStrategy);

        if (conversion == null)
            return;

        if (onProgress != null)
            conversion.OnProgress += (_, args) => onProgress(args.Percent);

        await conversion.Start();
    }
}
