using Application.Interfaces;
using Quartz;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class VideoSeederJob(ISeederService seederService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var jsonPath = Path.Combine(baseDir, "Seeding", "Videos.json");
        var videosFolder = Path.Combine(baseDir, "VideoForSeeder");

        await seederService.SeedVideosAsync(jsonPath, videosFolder);
    }
}
