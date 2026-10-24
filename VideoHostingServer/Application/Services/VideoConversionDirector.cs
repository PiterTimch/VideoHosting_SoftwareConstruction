using Xabe.FFmpeg;

namespace Application.Services;

public static class VideoConversionDirector
{
    public static IConversion? Build(
        IMediaInfo mediaInfo,
        int height,
        int threadCount,
        string outputPath,
        IVideoCodecStrategy codecStrategy)
    {
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        if (videoStream == null)
            return null;

        var ratio = (double)videoStream.Width / videoStream.Height;
        var width = (int)(height * ratio);
        if (width % 2 != 0)
            width++;

        var builder = new VideoConversionBuilder()
            .AddStream(conversion => conversion.AddStream(
                videoStream.SetSize(width, height).SetCodec(codecStrategy.Codec)));

        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();
        if (audioStream != null)
            builder.AddStream(conversion => conversion.AddStream(audioStream.SetCodec(AudioCodec.aac)));

        return builder
            .WithThreadCount(threadCount)
            .WithOutput(outputPath)
            .Build();
    }
}
