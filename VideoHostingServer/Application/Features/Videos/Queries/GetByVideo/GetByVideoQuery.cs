using Application.Models.Search;
using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.GetByVideo;

public record GetByVideoQuery(GetByModel Model, string? Token = null) : IRequest<VideoItemModel>;
