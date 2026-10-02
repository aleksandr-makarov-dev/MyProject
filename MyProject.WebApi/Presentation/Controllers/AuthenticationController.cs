using System.Security.Claims;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Features.Authentication.LoginByEmail;
using MyProject.WebApi.Application.Features.Authentication.LoginByExternalProvider;
using MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Presentation.Filters;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Presentation.Controllers;

[ApiController]
[Route("api/v1/authentication")]
public class AuthenticationController(
    IMediator mediator,
    SignInManager<User> signInManager,
    ILogger<AuthenticationController> logger) : ControllerBase
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

    [HttpGet("login/external/{provider}")]
    public async Task<IActionResult> LoginByExternalProvider(string provider,
        CancellationToken cancellationToken = default)
    {
        var providers = await signInManager.GetExternalAuthenticationSchemesAsync();

        if (providers.All(x => x.Name != provider))
        {
            throw new UnauthorizedAccessException("Provider is not supported");
        }

        var redirectUrl = Url.Action(nameof(LoginUserByExternalProviderCallback), "Authentication");
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [HttpGet("login/external/callback")]
    public async Task<IActionResult> LoginUserByExternalProviderCallback(CancellationToken cancellationToken)
    {
        var externalLoginInfo = await signInManager.GetExternalLoginInfoAsync();

        if (externalLoginInfo is null)
        {
            logger.LogWarning("External login callback received without external login information.");

            throw new UnauthorizedAccessException("No external login info found.");
        }

        var provider = externalLoginInfo.LoginProvider;
        var providerKey = externalLoginInfo.ProviderKey;

        var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("External login callback for provider {Provider} did not contain an email claim.",
                provider);

            throw new UnauthorizedAccessException("No email claim found.");
        }

        var command = new LoginByExternalProviderCommand
        {
            Email = email,
            Provider = provider,
            ProviderKey = providerKey
        };

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}