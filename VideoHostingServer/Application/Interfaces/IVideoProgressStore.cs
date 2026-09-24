using Application.Models.VideoProcessing;

namespace Application.Interfaces;

public interface IVideoProgressStore
{
    void Set(string trackingId, VideoProgressUpdate update);
    VideoProgressUpdate? Get(string trackingId);
}
