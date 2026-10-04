using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description =
                        "Enter your JWT token below.\n\n" +
                        "Example: **eyJhbGci...**\n\n" +
                        "(Do NOT prefix with 'Bearer ' — Swagger adds it automatically)"
                };

                var config    = context.ApplicationServices.GetRequiredService<IConfiguration>();
                var serverUrl = config["ApiServerUrl"];
                if (!string.IsNullOrEmpty(serverUrl))
                    document.Servers = [new OpenApiServer { Url = serverUrl }];
                else
                    document.SetReferenceHostDocument();

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                bool hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                bool hasAuthorize = metadata.OfType<IAuthorizeData>().Any();

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"),
                            []
                        }
                    }
                ];

                if (hasAuthorize && !hasAllowAnonymous)
                {
                    operation.Responses.TryAdd("401", new OpenApiResponse
                        { Description = "Unauthorized — valid JWT required" });
                    operation.Responses.TryAdd("403", new OpenApiResponse
                        { Description = "Forbidden — insufficient permissions" });
                }

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
