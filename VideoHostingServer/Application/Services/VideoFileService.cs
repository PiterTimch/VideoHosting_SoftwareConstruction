using Application.Interfaces;
using Application.Models.VideoProcessing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class VideoFileService : IVideoFileService
{
    private readonly VideoFileStorage _storage;
    private readonly VideoTranscoder _transcoder;

    public VideoFileService(IConfiguration configuration)
    {
        _storage = new VideoFileStorage(configuration);
        _transcoder = new VideoTranscoder(configuration, _storage);
    }

    public Task<string> SaveVideoAsync(IFormFile file)
    {
        return _storage.SaveAndProcessAsync(file, _transcoder.SaveVideoFromFilePathAsync);
    }

    public Task<string> SaveVideoFromFilePathAsync(string filePath)
    {
        return _transcoder.SaveVideoFromFilePathAsync(filePath);
    }

    public Task<string> SaveVideoWithProgressAsync(string filePath, Action<VideoProgressUpdate> onProgress)
    {
        return _transcoder.SaveVideoWithProgressAsync(filePath, onProgress);
    }

    public Task DeleteVideoAsync(string name)
    {
        return _storage.DeleteAsync(name);
    }
}
