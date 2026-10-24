using Application.Features.Videos.Queries.GetVideos;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain;
using Domain.Entities.Video;
using Infrastructure.Filters;
using Application.Jobs;
using Infrastructure.Jobs;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using FluentValidation;
using Application.Validators.Video;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Hangfire;
using Hangfire.PostgreSql;
using HeyRed.ImageSharp.Heif.Formats.Avif;
using SixLabors.ImageSharp;

namespace Infrastructure.ProgramConfiguration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("uk");

        services.AddHttpContextAccessor();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICookieAuthService, CookieAuthService>();
        services.AddScoped<ISmtpService, SmtpService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ISeederService, SeederService>();
        services.AddScoped<IRoleSeeder, RoleSeeder>();
        services.AddScoped<IUserSeeder, UserSeeder>();
        services.AddScoped<IVideoPrivacySeeder, VideoPrivacySeeder>();
        services.AddScoped<IVideoSeeder, VideoSeeder>();
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IVideoFileService, VideoFileService>();
        services.AddSingleton<IVideoProgressStore, VideoProgressStore>();
        services.AddSingleton<IVideoRecommendationService, VideoRecommendationService>();
        services.AddSingleton<IVideoAccessTokenService, VideoAccessTokenService>();
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
        services.AddScoped<IGenericRepository<VideoEntity, long>>(provider =>
            new VideoRepositoryDecorator(
                new GenericRepository<VideoEntity, long>(provider.GetRequiredService<AppDbContext>()),
                provider.GetRequiredService<IImageService>()));
        services.AddScoped<VideoProcessingJob>();

        Configuration.Default.Configure(new AvifConfigurationModule());

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(GetVideosQuery).Assembly);
        });

        services.AddSingleton<Application.Mappings.PortfolioMappingProfile>();
        services.AddSingleton<Application.Mappings.CommentMappingProfile>();
        services.AddSingleton<Application.Mappings.UserMapping>();
        services.AddSingleton<Application.Mappings.VideoMappingProfile>();

        services.AddQuartz(q =>
        {
            q.AddJobListener<SeederOrchestratorListener>();

            var migrationJobKey = new JobKey(nameof(DbMigrationJob));
            q.AddJob<DbMigrationJob>(opts => opts.WithIdentity(migrationJobKey));
            q.AddTrigger(opts => opts
                .ForJob(migrationJobKey)
                .WithIdentity("DbMigrationJob-trigger")
                .StartNow());

            var roleJobKey = new JobKey(nameof(RoleSeederJob));
            q.AddJob<RoleSeederJob>(opts => opts.WithIdentity(roleJobKey).StoreDurably());

            var userJobKey = new JobKey(nameof(UserSeederJob));
            q.AddJob<UserSeederJob>(opts => opts.WithIdentity(userJobKey).StoreDurably());

            var privacyJobKey = new JobKey(nameof(VideoPrivacySeederJob));
            q.AddJob<VideoPrivacySeederJob>(opts => opts.WithIdentity(privacyJobKey).StoreDurably());

            var videoJobKey = new JobKey(nameof(VideoSeederJob));
            q.AddJob<VideoSeederJob>(opts => opts.WithIdentity(videoJobKey).StoreDurably());
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        services.AddValidatorsFromAssemblyContaining<VideoCreateModelValidator>();
        services.AddValidatorsFromAssembly(typeof(GetVideosQuery).Assembly);

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue;
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });

        services.AddControllers();

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString))
        );

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = configuration.GetValue("VideoProcessing:HangfireWorkerCount", 1);
        });

        return services;
    }
}
