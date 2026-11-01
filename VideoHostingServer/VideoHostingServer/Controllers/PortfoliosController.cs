using Application.Features.Portfolio.Commands.TogglePortfolioSubscription;
using Application.Features.Portfolio.Queries.GetPortfolio;
using Application.Features.Portfolio.Commands.UpdatePortfolio;
using Application.Models.Portfolio;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VideoHostingServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController(IMediator mediator) : ControllerBase
{
    [HttpGet("get-by")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBy([FromQuery] long id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPortfolioQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("subscribe")]
    [Authorize]
    public async Task<IActionResult> ToggleSubscription([FromBody] PortfolioSubscriptionRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new TogglePortfolioSubscriptionCommand(request.PortfolioId), cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] PortfolioUpdateModel model, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdatePortfolioCommand(model), cancellationToken);
        return Ok(result);
    }
}

public record PortfolioSubscriptionRequest(long PortfolioId);
