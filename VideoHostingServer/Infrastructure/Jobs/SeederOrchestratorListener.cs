using Quartz;
using Quartz.Listener;

namespace Infrastructure.Jobs;

public class SeederOrchestratorListener : JobListenerSupport
{
    public override string Name => "SeederOrchestratorListener";

    private bool _privacySeeded = false;

    public override async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var jobKey = context.JobDetail.Key.Name;

        if (jobKey == nameof(DbMigrationJob))
        {
            await context.Scheduler.TriggerJob(new JobKey(nameof(VideoPrivacySeederJob)), cancellationToken);
        }
        else if (jobKey == nameof(VideoPrivacySeederJob))
        {
            _privacySeeded = true;
            if (_privacySeeded)
            {
                await context.Scheduler.TriggerJob(new JobKey(nameof(VideoSeederJob)), cancellationToken);
            }
        }
    }
}
