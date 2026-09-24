using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.GetVideos;

public record GetVideosQuery : IRequest<IEnumerable<VideoItemModel>>;
