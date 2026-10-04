using Application.Interfaces;
using Quartz;

namespace Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RoleSeederJob(ISeederService seederService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await seederService.SeedRolesAsync();
    }
}
