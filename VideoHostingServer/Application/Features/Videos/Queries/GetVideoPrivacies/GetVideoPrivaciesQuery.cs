using Application.Models.Video;
using MediatR;

namespace Application.Features.Videos.Queries.GetVideoPrivacies;

public record GetVideoPrivaciesQuery : IRequest<IEnumerable<VideoPrivacyItemModel>>;
