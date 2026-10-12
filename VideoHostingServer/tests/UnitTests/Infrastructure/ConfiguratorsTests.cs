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
        var authOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions>>().Value;
        authOptions.DefaultAuthenticateScheme.Should().Be(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);
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
    public async Task SwaggerConfigurator_DocumentAndOperationTransformers_ExecuteAllBranches()
    {
        var configWithUrl = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiServerUrl"] = "https://api.example.com"
            })
            .Build();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IConfiguration>(configWithUrl);
        builder.Services.AddSwaggerDocumentation();
        var appWithUrl = builder.Build();

        // 1. Document transformer with ApiServerUrl
        var doc = new Microsoft.OpenApi.OpenApiDocument();
        var docContextWithUrl = new Microsoft.AspNetCore.OpenApi.OpenApiDocumentTransformerContext
        {
            DocumentName = "v1",
            ApplicationServices = appWithUrl.Services,
            DescriptionGroups = new List<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescriptionGroup>()
        };
        await SwaggerConfigurator.TransformDocumentAsync(doc, docContextWithUrl, default);
        doc.Servers.Should().NotBeNullOrEmpty();
        doc.Servers![0].Url.Should().Be("https://api.example.com");

        // 2. Document transformer without ApiServerUrl
        var builderNoUrl = WebApplication.CreateBuilder();
        builderNoUrl.Services.AddSwaggerDocumentation();
        var appNoUrl = builderNoUrl.Build();
        var docNoUrl = new Microsoft.OpenApi.OpenApiDocument();
        var docContextNoUrl = new Microsoft.AspNetCore.OpenApi.OpenApiDocumentTransformerContext
        {
            DocumentName = "v1",
            ApplicationServices = appNoUrl.Services,
            DescriptionGroups = new List<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescriptionGroup>()
        };
        await SwaggerConfigurator.TransformDocumentAsync(docNoUrl, docContextNoUrl, default);

        // 3. Operation transformer with Authorize
        var authOp = new Microsoft.OpenApi.OpenApiOperation();
        var authDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor
        {
            EndpointMetadata = new List<object> { new Microsoft.AspNetCore.Authorization.AuthorizeAttribute() }
        };
        var authContext = new Microsoft.AspNetCore.OpenApi.OpenApiOperationTransformerContext
        {
            Description = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription { ActionDescriptor = authDescriptor },
            DocumentName = "v1",
            ApplicationServices = appWithUrl.Services
        };
        await SwaggerConfigurator.TransformOperationAsync(authOp, authContext, default);
        authOp.Responses.Should().ContainKey("401");
        authOp.Responses.Should().ContainKey("403");

        // 4. Operation transformer with AllowAnonymous
        var anonOp = new Microsoft.OpenApi.OpenApiOperation();
        var anonDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor
        {
            EndpointMetadata = new List<object>
            {
                new Microsoft.AspNetCore.Authorization.AuthorizeAttribute(),
                new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute()
            }
        };
        var anonContext = new Microsoft.AspNetCore.OpenApi.OpenApiOperationTransformerContext
        {
            Description = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription { ActionDescriptor = anonDescriptor },
            DocumentName = "v1",
            ApplicationServices = appWithUrl.Services
        };
        await SwaggerConfigurator.TransformOperationAsync(anonOp, anonContext, default);
        anonOp.Responses.Should().NotContainKey("401");

        // 5. Pipeline in Development and Production
        var prevEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var devApp = WebApplication.CreateBuilder().Build();
            devApp.UseSwaggerDocumentation();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            var prodApp = WebApplication.CreateBuilder().Build();
            prodApp.UseSwaggerDocumentation();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", prevEnv);
        }
    }
}
