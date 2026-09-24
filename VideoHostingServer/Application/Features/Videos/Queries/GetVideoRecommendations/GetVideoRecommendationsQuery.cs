using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.GetVideoRecommendations;

public record GetVideoRecommendationsQuery(VideoRecommendationRequest Model)
    : IRequest<IEnumerable<VideoItemModel>>;
