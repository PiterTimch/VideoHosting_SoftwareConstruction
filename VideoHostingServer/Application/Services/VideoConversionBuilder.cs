using Xabe.FFmpeg;

namespace Application.Services;

public class VideoConversionBuilder
{
    private readonly IConversion _conversion = FFmpeg.Conversions.New();
    private string? _outputPath;

    public VideoConversionBuilder AddStream(Action<IConversion> configure)
    {
        configure(_conversion);
        return this;
    }

    public VideoConversionBuilder WithThreadCount(int threadCount)
    {
        _conversion.UseMultiThread(threadCount);
        return this;
    }

    public VideoConversionBuilder WithOutput(string outputPath)
    {
        _outputPath = outputPath;
        return this;
    }

    public IConversion Build()
    {
        if (string.IsNullOrWhiteSpace(_outputPath))
            throw new InvalidOperationException("An output path must be set before building the conversion.");

        return _conversion.SetOutput(_outputPath);
    }
}
