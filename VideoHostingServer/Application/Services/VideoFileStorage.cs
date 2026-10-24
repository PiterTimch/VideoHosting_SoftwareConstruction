using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class VideoFileStorage
{
    private readonly string _videosDir;
    private readonly List<int> _videoSizes;

    public VideoFileStorage(IConfiguration configuration)
    {
        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot) ? Directory.GetCurrentDirectory() : mediaRoot;
        _videosDir = Path.Combine(basePath, configuration["VideosDir"]!);
        _videoSizes = configuration.GetSection("VideoSizes").Get<List<int>>()!;
        Directory.CreateDirectory(_videosDir);
    }

    public async Task<string> SaveAndProcessAsync(IFormFile file, Func<string, Task<string>> process)
    {
        var tempInputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));

        try
        {
            await using (var stream = new FileStream(tempInputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return await process(tempInputPath);
        }
        finally
        {
            if (File.Exists(tempInputPath))
                File.Delete(tempInputPath);
        }
    }

    public string GetOutputPath(int size, string name)
    {
        return Path.Combine(_videosDir, $"{size}_{name}");
    }

    public async Task DeleteAsync(string name)
    {
        var tasks = _videoSizes.Select(size => Task.Run(() =>
        {
            var path = GetOutputPath(size, name);
            if (File.Exists(path))
                File.Delete(path);
        }));

        await Task.WhenAll(tasks);
    }
}
