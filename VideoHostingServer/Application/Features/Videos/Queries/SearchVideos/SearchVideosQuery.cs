using Application.Models.Search;
using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.SearchVideos;

public record SearchVideosQuery(VideoSearchModel Model) 
    : IRequest<SearchResult<VideoItemModel>>;
