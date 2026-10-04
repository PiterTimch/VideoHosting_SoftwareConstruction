using Application.Interfaces;
using Quartz;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class UserSeederJob(ISeederService seederService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seeding", "Users.json");
        await seederService.SeedUsersAsync(jsonPath);
    }
}
