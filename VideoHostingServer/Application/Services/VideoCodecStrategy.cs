using Xabe.FFmpeg;

namespace Application.Services;

public interface IVideoCodecStrategy
{
    VideoCodec Codec { get; }
}

public sealed class H264VideoCodecStrategy : IVideoCodecStrategy
{
    public VideoCodec Codec => VideoCodec.h264;
}

public sealed class H265VideoCodecStrategy : IVideoCodecStrategy
{
    public VideoCodec Codec => VideoCodec.hevc;
}

public static class VideoCodecStrategyFactory
{
    public static IVideoCodecStrategy Create(string? codec)
    {
        return codec?.ToLowerInvariant() switch
        {
            "h265" => new H265VideoCodecStrategy(),
            _ => new H264VideoCodecStrategy()
        };
    }
}
