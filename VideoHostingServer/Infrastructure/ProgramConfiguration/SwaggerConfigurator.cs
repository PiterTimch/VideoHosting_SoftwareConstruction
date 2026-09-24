using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Threading.Tasks;

namespace Infrastructure.ProgramConfiguration;

public static class SwaggerConfigurator
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var config    = context.ApplicationServices.GetRequiredService<IConfiguration>();
                var serverUrl = config["ApiServerUrl"];
                if (!string.IsNullOrEmpty(serverUrl))
                    document.Servers = [new OpenApiServer { Url = serverUrl }];
                else
                    document.SetReferenceHostDocument();

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        var routePrefix    = isDevelopment ? "swagger" : "swagger";
        var openApiRoute   = isDevelopment ? "/openapi/{documentName}.json" : "/swagger/{documentName}.json";
        var schemaEndpoint = isDevelopment ? "../openapi/v1.json" : "v1.json";

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = routePrefix;
            options.SwaggerEndpoint(schemaEndpoint, "VideoHosting API v1");
            options.OAuthUsePkce();
            options.EnablePersistAuthorization();
        });

        if (app is IEndpointRouteBuilder endpoints)
        {
            endpoints.MapOpenApi(openApiRoute);

            endpoints.MapScalarApiReference(options =>
            {
                options.WithTitle("VideoHosting API Documentation")
                       .WithTheme(ScalarTheme.Moon)
                       .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        return app;
    }
}
