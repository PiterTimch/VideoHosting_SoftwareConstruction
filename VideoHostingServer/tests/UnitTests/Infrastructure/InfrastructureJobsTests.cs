using Application.Interfaces;
using Infrastructure.Jobs;
using Moq;
using Quartz;
using Xunit;

namespace UnitTests.Infrastructure;

public class InfrastructureJobsTests
{
    private readonly Mock<ISeederService> _seederServiceMock = new();
    private readonly Mock<IJobExecutionContext> _jobContextMock = new();

    [Fact]
    public async Task DbMigrationJob_CallsUpdateDatabase()
    {
        var job = new DbMigrationJob(_seederServiceMock.Object);
        await job.Execute(_jobContextMock.Object);

        _seederServiceMock.Verify(s => s.UpdateDatabase(), Times.Once);
    }

    [Fact]
    public async Task RoleSeederJob_CallsSeedRolesAsync()
    {
        var job = new RoleSeederJob(_seederServiceMock.Object);
        await job.Execute(_jobContextMock.Object);

        _seederServiceMock.Verify(s => s.SeedRolesAsync(), Times.Once);
    }

    [Fact]
    public async Task UserSeederJob_CallsSeedUsersAsync()
    {
        var job = new UserSeederJob(_seederServiceMock.Object);
        await job.Execute(_jobContextMock.Object);

        _seederServiceMock.Verify(s => s.SeedUsersAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task VideoPrivacySeederJob_CallsSeedVideoPrivaciesAsync()
    {
        var job = new VideoPrivacySeederJob(_seederServiceMock.Object);
        await job.Execute(_jobContextMock.Object);

        _seederServiceMock.Verify(s => s.SeedVideoPrivaciesAsync(), Times.Once);
    }

    [Fact]
    public async Task VideoSeederJob_CallsSeedVideosAsync()
    {
        var job = new VideoSeederJob(_seederServiceMock.Object);
        await job.Execute(_jobContextMock.Object);

        _seederServiceMock.Verify(s => s.SeedVideosAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
