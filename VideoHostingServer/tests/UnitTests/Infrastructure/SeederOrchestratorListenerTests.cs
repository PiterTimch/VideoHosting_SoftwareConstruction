using FluentAssertions;
using Infrastructure.Jobs;
using Moq;
using Quartz;
using Xunit;

namespace UnitTests.Infrastructure;

public class SeederOrchestratorListenerTests
{
    private static IJobExecutionContext Context(string jobName, IScheduler scheduler)
    {
        var detail = new Mock<IJobDetail>();
        detail.Setup(d => d.Key).Returns(new JobKey(jobName));
        var ctx = new Mock<IJobExecutionContext>();
        ctx.Setup(c => c.JobDetail).Returns(detail.Object);
        ctx.Setup(c => c.Scheduler).Returns(scheduler);
        return ctx.Object;
    }

    [Fact]
    public async Task JobWasExecuted_TriggersExpectedJobs()
    {
        var scheduler = new Mock<IScheduler>();
        var listener = new SeederOrchestratorListener();
        listener.Name.Should().Be("SeederOrchestratorListener");

        await listener.JobWasExecuted(Context(nameof(DbMigrationJob), scheduler.Object), null);
        scheduler.Verify(s => s.TriggerJob(It.Is<JobKey>(k => k.Name == nameof(RoleSeederJob)), default), Times.Once);

        await listener.JobWasExecuted(Context(nameof(RoleSeederJob), scheduler.Object), null);
        scheduler.Verify(s => s.TriggerJob(It.Is<JobKey>(k => k.Name == nameof(UserSeederJob)), default), Times.Once);
        scheduler.Verify(s => s.TriggerJob(It.Is<JobKey>(k => k.Name == nameof(VideoPrivacySeederJob)), default), Times.Once);

        await listener.JobWasExecuted(Context(nameof(VideoPrivacySeederJob), scheduler.Object), null);
        scheduler.Verify(s => s.TriggerJob(It.Is<JobKey>(k => k.Name == nameof(VideoSeederJob)), default), Times.Never);

        await listener.JobWasExecuted(Context(nameof(UserSeederJob), scheduler.Object), null);
        scheduler.Verify(s => s.TriggerJob(It.Is<JobKey>(k => k.Name == nameof(VideoSeederJob)), default), Times.Once);
    }
}
