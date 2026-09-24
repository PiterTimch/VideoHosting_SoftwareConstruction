using Application.Interfaces;
using Application.Models.VideoProcessing;
using Application.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Application.Services;

public class VideoFileService : IVideoFileService
{
    private readonly string _videosDir;
    private readonly string _ffmpegPath;
    private readonly List<int> _videoSizes;
    private readonly int _ffmpegThreads;
    private readonly TimeSpan _progressUpdateInterval;
    private readonly double _minProgressPercentDelta;

    public VideoFileService(IConfiguration configuration)
    {
        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot)
            ? Directory.GetCurrentDirectory()
            : mediaRoot;
        _videosDir = Path.Combine(basePath, configuration["VideosDir"]!);
        _videoSizes = configuration.GetSection("VideoSizes").Get<List<int>>()!;
        _ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "FFmpeg");
        _ffmpegThreads = configuration.GetValue("VideoProcessing:FfmpegThreads", 2);
        _progressUpdateInterval = TimeSpan.FromSeconds(
            configuration.GetValue("VideoProcessing:ProgressUpdateIntervalSeconds", 3));
        _minProgressPercentDelta = configuration.GetValue("VideoProcessing:MinProgressPercentDelta", 2);

        Directory.CreateDirectory(_videosDir);

        FFmpeg.SetExecutablesPath(_ffmpegPath);
    }

    public async Task<string> SaveVideoAsync(IFormFile file)
    {
        var tempInputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        using (var stream = new FileStream(tempInputPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            return await SaveVideoFromFilePathAsync(tempInputPath);
        }
        finally
        {
            if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
        }
    }

    public async Task<string> SaveVideoFromFilePathAsync(string filePath)
    {
        return await SaveVideoWithProgressAsync(filePath, _ => { });
    }

    public async Task<string> SaveVideoWithProgressAsync(string filePath, Action<VideoProgressUpdate> onProgress)
    {
        string ffmpegFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
        string ffmpegFullExecutablePath = Path.Combine(_ffmpegPath, ffmpegFileName);

        var throttler = new ProgressThrottler(onProgress, _progressUpdateInterval, _minProgressPercentDelta);

        if (!File.Exists(ffmpegFullExecutablePath))
        {
            throttler.Report(new VideoProgressUpdate { Status = "Downloading FFmpeg...", Percentage = 0 }, force: true);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _ffmpegPath);
        }

        FFmpeg.SetExecutablesPath(_ffmpegPath);

        var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
        string baseFileName = $"{Guid.NewGuid()}.mp4";
        var startTime = DateTime.UtcNow;
        var totalSizes = _videoSizes.Count;

        for (var index = 0; index < totalSizes; index++)
        {
            var size = _videoSizes[index];
            var completedSizes = index;

            await ProcessVideoAsync(filePath, baseFileName, size, mediaInfo, (sizePercent) =>
            {
                var totalProgress = ((completedSizes + sizePercent / 100.0) / totalSizes) * 100;

                string remainingTimeText = "Calculating...";
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

    private async Task ProcessVideoAsync(string inputPath, string baseName, int height, IMediaInfo mediaInfo, Action<double>? onProgress = null)
    {
        var outputPath = Path.Combine(_videosDir, $"{height}_{baseName}");
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        if (videoStream == null) return;

        double ratio = (double)videoStream.Width / videoStream.Height;
        int width = (int)(height * ratio);
        if (width % 2 != 0) width++;

        var vStream = videoStream
            .SetSize(width, height)
            .SetCodec(VideoCodec.h264);

        var conversion = FFmpeg.Conversions.New()
            .AddStream(vStream)
            .UseMultiThread(_ffmpegThreads);

        if (audioStream != null)
        {
            conversion.AddStream(audioStream.SetCodec(AudioCodec.aac));
        }

        if (onProgress != null)
        {
            conversion.OnProgress += (_, args) => onProgress(args.Percent);
        }

        await conversion.SetOutput(outputPath).Start();
    }

    public async Task DeleteVideoAsync(string name)
    {
        var tasks = _videoSizes.Select(size => Task.Run(() =>
        {
            var path = Path.Combine(_videosDir, $"{size}_{name}");
            if (File.Exists(path)) File.Delete(path);
        }));

        await Task.WhenAll(tasks);
    }
}
