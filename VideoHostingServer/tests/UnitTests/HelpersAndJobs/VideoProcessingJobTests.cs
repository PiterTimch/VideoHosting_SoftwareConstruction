using Application.Hubs;
using Application.Interfaces;
using Application.Jobs;
using Application.Models.VideoProcessing;
using Domain.Entities.Video;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using UnitTests.Common;
using Xunit;

namespace UnitTests.HelpersAndJobs;

public class VideoProcessingJobTests
{
    private readonly Mock<IHubContext<VideoProgressHub>> _hubContextMock;
    private readonly Mock<IVideoFileService> _videoFileServiceMock;
    private readonly Mock<IVideoProgressStore> _progressStoreMock;
    private readonly Mock<ILogger<VideoProcessingJob>> _loggerMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;

    public VideoProcessingJobTests()
    {
        _hubContextMock = new Mock<IHubContext<VideoProgressHub>>();
        _videoFileServiceMock = new Mock<IVideoFileService>();
        _progressStoreMock = new Mock<IVideoProgressStore>();
        _loggerMock = new Mock<ILogger<VideoProcessingJob>>();
        _hubClientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);
        _hubClientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenTempFileDoesNotExist_ThrowsFileNotFoundException()
    {
        var context = TestDbContextHelper.CreateContext();
        var repo = new GenericRepository<VideoEntity, long>(context, null);

        var job = new VideoProcessingJob(
            _hubContextMock.Object,
            _videoFileServiceMock.Object,
            repo,
            _progressStoreMock.Object,
            _loggerMock.Object);

        await FluentActions.Invoking(() => job.ProcessVideoAsync(1, "non_existent.mp4", "tid-1"))
            .Should().ThrowAsync<FileNotFoundException>();

        _progressStoreMock.Verify(p => p.Set("tid-1", It.Is<VideoProgressUpdate>(u => u.Status.StartsWith("Помилка"))), Times.Once);
    }

    [Fact]
    public async Task ProcessVideoAsync_Success_ProcessesVideoUpdatesDbAndSendsCompletion()
    {
        var context = TestDbContextHelper.CreateContext();
        var video = new VideoEntity { Id = 10, Title = "Processing", Video = "processing...", IsDeleted = false };
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        var repo = new GenericRepository<VideoEntity, long>(context, null);

        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "dummy video data");

            _videoFileServiceMock.Setup(v => v.SaveVideoWithProgressAsync(tempFile, It.IsAny<Action<VideoProgressUpdate>>()))
                .ReturnsAsync("saved_result.mp4");

            var job = new VideoProcessingJob(
                _hubContextMock.Object,
                _videoFileServiceMock.Object,
                repo,
                _progressStoreMock.Object,
                _loggerMock.Object);

            await job.ProcessVideoAsync(10, tempFile, "tid-success");

            var updated = await context.Videos.FindAsync(10L);
            updated!.Video.Should().Be("saved_result.mp4");

            _progressStoreMock.Verify(p => p.Set("tid-success", It.Is<VideoProgressUpdate>(u => u.Percentage == 100)), Times.Once);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
