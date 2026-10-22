using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Videos.Queries.GetVideoStream;

public class GetVideoStreamQuery(
    string videoName,
    string? token,
    string? rangeHeader) : IRequest<IActionResult>
{
    public string VideoName { get; } = videoName;
    public string? Token { get; } = token;
    public string? RangeHeader { get; } = rangeHeader;
}
