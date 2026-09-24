using Domain.Entities.Video;

namespace Application.Interfaces;

public interface IVideoRecommendationService
{
    int ComputeScore(VideoEntity source, VideoEntity candidate);
}
