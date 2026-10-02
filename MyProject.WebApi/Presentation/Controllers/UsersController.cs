using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Features.Users.GetMyProfile;

namespace MyProject.WebApi.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(result);
    }
}