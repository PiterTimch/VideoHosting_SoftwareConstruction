using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.AutocompleteVideos;

public record AutocompleteVideosQuery(string Q) : IRequest<IEnumerable<VideoAutocompleteModel>>;
