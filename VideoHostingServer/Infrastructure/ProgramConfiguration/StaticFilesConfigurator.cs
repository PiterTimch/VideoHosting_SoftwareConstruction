using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Infrastructure.ProgramConfiguration;

public static class StaticFilesConfigurator
{
    public static IApplicationBuilder UseImagesDirectory(this IApplicationBuilder app, IConfiguration configuration)
    {
        var dir = configuration["ImagesDir"]!;
        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot)
            ? Directory.GetCurrentDirectory()
            : mediaRoot;
        var path = Path.Combine(basePath, dir);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = $"/{dir}"
        });

        return app;
    }

    public static IApplicationBuilder UseVideosDirectory(this IApplicationBuilder app, IConfiguration configuration)
    {
        var dir = configuration["VideosDir"]!;
        var mediaRoot = configuration["MediaRoot"];
        var basePath = string.IsNullOrEmpty(mediaRoot)
            ? Directory.GetCurrentDirectory()
            : mediaRoot;
        var path = Path.Combine(basePath, dir);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(path),
            RequestPath = $"/{dir}"
        });

        return app;
    }
}
