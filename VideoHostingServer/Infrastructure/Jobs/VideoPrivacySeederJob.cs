using Application.Interfaces;
using Quartz;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class VideoPrivacySeederJob(ISeederService seederService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await seederService.SeedVideoPrivaciesAsync();
    }
}
