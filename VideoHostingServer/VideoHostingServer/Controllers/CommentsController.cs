using Application.Features.Comments.Commands.CreateComments;
using Application.Features.Comments.Commands.DeleteComment;
using Application.Features.Comments.Commands.UpdateComment;
using Application.Features.Comments.Queries.GetCommentsReplies;
using Application.Features.Comments.Queries.GetVideoComments;
using Application.Models.Comments;
using Application.Models.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VideoHostingServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("video/{videoId:long}")]
    [AllowAnonymous]
    public async Task<ActionResult<SearchResult<CommentsItemModal>>> GetByVideo(
        long videoId,
        [FromQuery] BaseSearchParamsModel model
    )
    {
        var result = await mediator.Send(new GetVideoCommentsQuery(videoId, model));
        return Ok(result);
    }

    [HttpGet("{parentId:long}/replies")]
    [AllowAnonymous]
    public async Task<ActionResult<SearchResult<CommentsItemModal>>> GetReplies(
        long parentId,
        [FromQuery] BaseSearchParamsModel model
    )
    {
        var result = await mediator.Send(new GetCommentRepliesQuery(parentId, model));
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentsItemModal>> Create(
        [FromBody] CreateCommentRequest request
    )
    {
        var command = new CreateCommentCommand(request.Content, request.VideoId, request.ParentId);

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<ActionResult<CommentsItemModal>> Update(
        long id,
        [FromBody] UpdateCommentRequest request
    )
    {
        var result = await mediator.Send(new UpdateCommentCommand(id, request.Content));
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        await mediator.Send(new DeleteCommentCommand(id));
        return NoContent();
    }
}

public record CreateCommentRequest(string Content, long VideoId, long? ParentId);
public record UpdateCommentRequest(string Content);
