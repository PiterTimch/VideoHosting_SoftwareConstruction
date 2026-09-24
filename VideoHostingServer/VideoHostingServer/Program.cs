using Infrastructure.Middlewares;
using Infrastructure.ProgramConfiguration;
using Application.Hubs;
using Serilog;
using Serilog.Events;
using Hangfire;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = null;
    });

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
    );

    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();

    var app = builder.Build();

    app.UseImagesDirectory(builder.Configuration);
    app.UseVideosDirectory(builder.Configuration);

    app.UseSwaggerDocumentation();

    app.UseSerilogRequestLogging();  

    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    app.UseCors("AllowAll");         
    
    app.UseHangfireDashboard();      

    app.MapControllers();
    app.MapHub<VideoProgressHub>("/videoProgressHub");

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException && ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
