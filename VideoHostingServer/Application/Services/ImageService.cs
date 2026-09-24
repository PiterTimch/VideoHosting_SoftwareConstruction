using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Application.Services;

public class ImageService(IConfiguration configuration) : IImageService
{
    private static readonly Dictionary<string, Func<IImageEncoder>> EncoderFactories = new()
    {
        ["webp"] = () => new WebpEncoder(),
        ["avif"] = () => new AVIFEncoder { CQLevel = 28 }
    };

    private string GetImagesBasePath()
    {
        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot)
            ? Directory.GetCurrentDirectory()
            : mediaRoot;
        return Path.Combine(basePath, configuration["ImagesDir"]!);
    }

    private List<string> GetOutputFormats()
    {
        var formats = configuration.GetSection("ImageFormats").Get<List<string>>();
        return formats is { Count: > 0 } ? formats : ["webp"];
    }

    public async Task DeleteImageAsync(string name)
    {
        var sizes = configuration.GetRequiredSection("ImageSizes").Get<List<int>>()!;
        var formats = GetOutputFormats();
        var dir = GetImagesBasePath();
        var pureName = Path.GetFileNameWithoutExtension(name);

        Task[] tasks = sizes
            .SelectMany(size => formats.Select(format => (size, format)))
            .AsParallel()
            .Select(x => Task.Run(() =>
            {
                var path = Path.Combine(dir, $"{x.size}_{pureName}.{x.format}");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    public async Task<string> SaveImageFromUrlAsync(string imageUrl)
    {
        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
        return await SaveImageAsync(imageBytes);
    }

    public async Task<string> SaveImageAsync(IFormFile file)
    {
        using MemoryStream ms = new();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        return await SaveImageAsync(bytes);
    }

    public async Task<string> SaveImageFromBase64Async(string input)
    {
        var base64Data = input.Contains(',')
            ? input[(input.IndexOf(',') + 1)..]
            : input;

        var imageBytes = Convert.FromBase64String(base64Data);
        return await SaveImageAsync(imageBytes);
    }

    private async Task<string> SaveImageAsync(byte[] bytes)
    {
        var randomName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        var sizes = configuration.GetRequiredSection("ImageSizes").Get<List<int>>()!;
        var formats = GetOutputFormats();

        Task[] tasks = sizes
            .SelectMany(size => formats.Select(format => (size, format)))
            .Select(x => SaveImageAsync(bytes, randomName, x.size, x.format))
            .ToArray();

        await Task.WhenAll(tasks);

        return $"{randomName}.webp";
    }

    private async Task SaveImageAsync(byte[] bytes, string name, int size, string format)
    {
        var path = Path.Combine(GetImagesBasePath(), $"{size}_{name}.{format}");

        using var image = Image.Load(bytes);
        image.Mutate(imgContext => imgContext.Resize(new ResizeOptions
        {
            Size = new Size(size, size),
            Mode = ResizeMode.Max
        }));

        var encoder = EncoderFactories[format]();
        await image.SaveAsync(path, encoder);
    }
}
