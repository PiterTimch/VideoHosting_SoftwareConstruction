using Application.Features.Videos.Queries.GetVideos;
using Application.Features.Videos.Commands.CreateVideo;
using Application.Interfaces;
using Application.Models.Video;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Videos.Commands.UpdateVideo;
using Application.Features.Videos.Commands.DeleteVideo;
using Application.Models.Search;
using Application.Features.Videos.Queries.SearchVideos;
using Application.Models.VideoProcessing;
using Application.Features.Videos.Queries.GetByVideo;
using Application.Features.Videos.Queries.GetVideoPrivacies;
using Application.Features.Videos.Commands.IncrementView;
using Application.Features.Videos.Queries.GetVideoRecommendations;
using Application.Features.Videos.Queries.AutocompleteVideos;
using Microsoft.Extensions.Logging;

namespace VideoHostingServer.Controllers;

public class TestVideoSavingCommand : IRequest
{
    public IFormFile File { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class VideosController(
    IMediator mediator,
    IVideoFileService videoFileService,
    IVideoProgressStore progressStore,
    ILogger<VideosController> logger) : ControllerBase
{
    [HttpPost("TestVideoSaving")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<string>> TestVideoSaving([FromForm] TestVideoSavingCommand model)
    {
        var result = await videoFileService.SaveVideoAsync(model.File);
        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoItemModel>>> GetAll()
    {
        var query = new GetVideosQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("get-by")]
    [AllowAnonymous]
    public async Task<ActionResult<VideoItemModel>> GetById([FromQuery] GetByModel model)
    {
        var query = new GetByVideoQuery(model);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<SearchResult<VideoItemModel>>> Search(
        [FromQuery] VideoSearchModel model)
    {
        var query = new SearchVideosQuery(model);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VideoProcessingResult>> Create([FromForm] VideoCreateModel model)
    {
        var command = new CreateVideoCommand(model);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VideoProcessingResult>> Update([FromForm] VideoUpdateModel model)
    {
        var command = new UpdateVideoCommand(model);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete]
    [AllowAnonymous]
    public async Task<ActionResult> Delete([FromBody] VideoDeleteModel model)
    {
        var command = new DeleteVideoCommand(model);
        await mediator.Send(command);
        return Ok();
    }

    [HttpGet("privacies")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoPrivacyItemModel>>> GetPrivacies()
    {
        var query = new GetVideoPrivaciesQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/view")]
    [AllowAnonymous]
    public async Task<ActionResult> IncrementView([FromRoute] long id)
    {
        var command = new IncrementViewCommand(id);
        await mediator.Send(command);
        return Ok();
    }

    [HttpGet("recommendations")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoItemModel>>> GetRecommendations([FromQuery] VideoRecommendationRequest model)
    {
        var query = new GetVideoRecommendationsQuery(model);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("autocomplete")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<VideoAutocompleteModel>>> Autocomplete([FromQuery] string q)
    {
        var query = new AutocompleteVideosQuery(q);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("progress/{trackingId}")]
    [AllowAnonymous]
    public ActionResult<VideoProgressUpdate> GetProcessingProgress([FromRoute] string trackingId)
    {
        logger.LogInformation("[VideoProgress] API GET progress trackingId={TrackingId}", trackingId);

        var progress = progressStore.Get(trackingId);
        if (progress is null)
        {
            logger.LogDebug("[VideoProgress] API GET progress NOT FOUND trackingId={TrackingId}", trackingId);
            return NotFound();
        }

        return Ok(progress);
    }
}
