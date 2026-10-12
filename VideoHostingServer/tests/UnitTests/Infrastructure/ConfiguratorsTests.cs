using Application.Constants;
using Domain;
using Domain.Entities.Identity;
using FluentAssertions;
using Infrastructure.ProgramConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.Infrastructure;

public class ConfiguratorsTests
{
    [Fact]
    public void AddIdentityConfiguration_RegistersIdentityAndJwt()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForTestingJwtTokenGeneration1234567890!"
            })
            .Build();

        services.AddIdentityConfiguration(config);
        var provider = services.BuildServiceProvider();

        provider.GetService<Microsoft.AspNetCore.Identity.UserManager<UserEntity>>().Should().NotBeNull();
    }

    [Fact]
    public async Task JwtBearer_OnMessageReceived_ReadsCookieWhenHeaderMissing()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongSecretKeyForTestingJwtTokenGeneration1234567890!"
            })
            .Build();
        services.AddIdentityConfiguration(config);
        var provider = services.BuildServiceProvider();

        var optionsMonitor = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>>();
        var options = optionsMonitor.Get(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);

        var http = new DefaultHttpContext();
        http.Request.Headers.Cookie = $"{AuthConstants.AuthCookieName}=cookie-token";
        var context = new Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext(
            http,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, null, typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler)),
            options);

        await options.Events.OnMessageReceived(context);
        context.Token.Should().Be("cookie-token");

        http.Request.Headers.Authorization = "Bearer header-token";
        var context2 = new Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext(
            http,
            new Microsoft.AspNetCore.Authentication.AuthenticationScheme(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, null, typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler)),
            options);
        await options.Events.OnMessageReceived(context2);
        context2.Token.Should().BeNull();
    }

    [Fact]
    public void StaticFilesConfigurator_CreatesDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), "static_" + Guid.NewGuid());
        Directory.CreateDirectory(root);
        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MediaRoot"] = root,
                    ["ImagesDir"] = "images",
                    ["VideosDir"] = "videos"
                })
                .Build();

            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();
            app.UseImagesDirectory(config);
            app.UseVideosDirectory(config);

            Directory.Exists(Path.Combine(root, "images")).Should().BeTrue();
            Directory.Exists(Path.Combine(root, "videos")).Should().BeTrue();

            var configEmpty = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["MediaRoot"] = "",
                    ["ImagesDir"] = Path.Combine("tmp_img_" + Guid.NewGuid().ToString("N")[..8]),
                    ["VideosDir"] = Path.Combine("tmp_vid_" + Guid.NewGuid().ToString("N")[..8])
                })
                .Build();
            app.UseImagesDirectory(configEmpty);
            app.UseVideosDirectory(configEmpty);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SwaggerConfigurator_RegistersAndAppliesPipeline()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSwaggerDocumentation();
        var app = builder.Build();
        app.UseSwaggerDocumentation();
        app.Should().NotBeNull();
    }
}
