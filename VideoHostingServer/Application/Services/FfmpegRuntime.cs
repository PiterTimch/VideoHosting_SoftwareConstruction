using System.Runtime.InteropServices;
using Application.Models.VideoProcessing;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Application.Services;

public class FfmpegRuntime
{
    private readonly string _ffmpegPath;

    public FfmpegRuntime()
    {
        _ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "FFmpeg");
        FFmpeg.SetExecutablesPath(_ffmpegPath);
    }

    public async Task EnsureAvailableAsync(Action<VideoProgressUpdate> onProgress)
    {
        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
        var executablePath = Path.Combine(_ffmpegPath, executableName);
        if (File.Exists(executablePath))
            return;

        onProgress(new VideoProgressUpdate { Status = "Downloading FFmpeg...", Percentage = 0 });
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _ffmpegPath);
        FFmpeg.SetExecutablesPath(_ffmpegPath);
    }
}
