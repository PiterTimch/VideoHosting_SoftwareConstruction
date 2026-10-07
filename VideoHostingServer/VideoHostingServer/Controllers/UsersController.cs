using Application.Constants;
using Application.Models.Search;
using Application.Models.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VideoHostingServer.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<SearchResult<UserItemModel>>> SearchUsers([FromQuery] UserSearchModel model)
    {
        // var query = new SearchUsersQuery(model);
        // var result = await mediator.Send(query);
        // return Ok(result);
        throw new NotImplementedException("SearchUsers query is missing in Application layer.");
    }

    [HttpPut]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> EditUser([FromForm] UserEditModel model)
    {
        // var command = new EditUserCommand(model);
        // var result = await mediator.Send(command);
        // return Ok(new { Token = result });
        throw new NotImplementedException("EditUser command is missing in Application layer.");
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteUser([FromBody] UserDeleteModel model)
    {
        // var command = new DeleteUserCommand(model.Id);
        // await mediator.Send(command);
        // return Ok();
        throw new NotImplementedException("DeleteUser command is missing in Application layer.");
    }
}
