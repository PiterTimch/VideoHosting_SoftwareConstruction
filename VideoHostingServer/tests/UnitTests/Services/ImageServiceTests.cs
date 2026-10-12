using Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace UnitTests.Services;

public class ImageServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly Mock<IConfiguration> _configMock;
    private const string Base64Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

    public ImageServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "ImageServiceTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["MediaRoot"]).Returns(_testDir);
        _configMock.Setup(c => c["ImagesDir"]).Returns("");

        var imageSizesSection = new Mock<IConfigurationSection>();
        imageSizesSection.Setup(s => s.Value).Returns((string?)null);
        _configMock.Setup(c => c.GetSection("ImageSizes")).Returns(imageSizesSection.Object);

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"MediaRoot", _testDir},
            {"ImagesDir", ""},
            {"ImageSizes:0", "50"},
            {"ImageSizes:1", "100"},
            {"ImageFormats:0", "webp"}
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["MediaRoot"]).Returns(_testDir);
        _configMock.Setup(c => c["ImagesDir"]).Returns("");
        _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns<string>(key => config.GetSection(key));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }

    [Fact]
    public async Task SaveImageFromBase64Async_WithAndWithoutPrefix_SavesWebpFiles()
    {
        var service = new ImageService(_configMock.Object);

        var inputWithPrefix = "data:image/png;base64," + Base64Png;
        var fileName1 = await service.SaveImageFromBase64Async(inputWithPrefix);
        fileName1.Should().EndWith(".webp");
        File.Exists(Path.Combine(_testDir, $"50_{fileName1}")).Should().BeTrue();
        File.Exists(Path.Combine(_testDir, $"100_{fileName1}")).Should().BeTrue();

        var fileName2 = await service.SaveImageFromBase64Async(Base64Png);
        fileName2.Should().EndWith(".webp");
        File.Exists(Path.Combine(_testDir, $"50_{fileName2}")).Should().BeTrue();

        await service.DeleteImageAsync(fileName1);
        File.Exists(Path.Combine(_testDir, $"50_{fileName1}")).Should().BeFalse();
    }

    [Fact]
    public async Task SaveImageAsync_WithFormFile_SavesResizedImages()
    {
        var service = new ImageService(_configMock.Object);
        var bytes = Convert.FromBase64String(Base64Png);
        var stream = new MemoryStream(bytes);

        var formFile = new FormFile(stream, 0, bytes.Length, "Data", "pixel.png");

        var fileName = await service.SaveImageAsync(formFile);

        fileName.Should().EndWith(".webp");
        File.Exists(Path.Combine(_testDir, $"50_{fileName}")).Should().BeTrue();

        await service.DeleteImageAsync(fileName);
        File.Exists(Path.Combine(_testDir, $"50_{fileName}")).Should().BeFalse();
    }
}
