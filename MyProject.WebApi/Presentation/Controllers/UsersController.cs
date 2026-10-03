using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Features.Users.GetMyProfile;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Identity;

namespace MyProject.WebApi.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HasPermission(Permissions.Users.Me)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(result);
    }
}