using Quartz;
using Quartz.Listener;

namespace Infrastructure.Jobs;

public class SeederOrchestratorListener : JobListenerSupport
{
    public override string Name => "SeederOrchestratorListener";

    private bool _privacySeeded = false;
    private bool _userSeeded = false;
    private readonly object _syncLock = new();

    public override async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var jobKey = context.JobDetail.Key.Name;

        if (jobKey == nameof(DbMigrationJob))
        {
            await context.Scheduler.TriggerJob(new JobKey(nameof(RoleSeederJob)), cancellationToken);
        }
        else if (jobKey == nameof(RoleSeederJob))
        {
            await context.Scheduler.TriggerJob(new JobKey(nameof(UserSeederJob)), cancellationToken);
            await context.Scheduler.TriggerJob(new JobKey(nameof(VideoPrivacySeederJob)), cancellationToken);
        }
        else if (jobKey == nameof(VideoPrivacySeederJob) || jobKey == nameof(UserSeederJob))
        {
            bool triggerVideo = false;

            lock (_syncLock)
            {
                if (jobKey == nameof(VideoPrivacySeederJob)) _privacySeeded = true;
                if (jobKey == nameof(UserSeederJob)) _userSeeded = true;

                if (_privacySeeded && _userSeeded)
                {
                    triggerVideo = true;
                }
            }

            if (triggerVideo)
            {
                await context.Scheduler.TriggerJob(new JobKey(nameof(VideoSeederJob)), cancellationToken);
            }
        }
    }
}
