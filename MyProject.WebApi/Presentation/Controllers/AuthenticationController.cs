using Mediator;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Features.Authentication.LoginByEmail;
using MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;
using MyProject.WebApi.Presentation.Filters;

namespace MyProject.WebApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/authentication")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{
    [HttpPost("login/email")]
    [Validate(typeof(LoginByEmailCommand))]
    public async Task<IActionResult> LoginByEmail([FromBody] LoginByEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPost("login/email/verify")]
    public async Task<IActionResult> VerifyLoginByEmail([FromBody] VerifyLoginByEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}