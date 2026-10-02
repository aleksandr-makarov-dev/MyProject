using System.Security.Claims;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyProject.WebApi.Application.Features.Authentication.LoginByEmail;
using MyProject.WebApi.Application.Features.Authentication.LoginByExternalProvider;
using MyProject.WebApi.Application.Features.Authentication.Logout;
using MyProject.WebApi.Application.Features.Authentication.RotateRefreshToken;
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
    private const string RefreshTokenCookieName = "refresh_token";

    [HttpPost("login/email")]
    [Validate(typeof(LoginByEmailCommand))]
    public async Task<IActionResult> LoginByEmail([FromBody] LoginByEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPost("login/email/verify")]
    [Validate(typeof(VerifyLoginByEmailCommand))]
    public async Task<IActionResult> VerifyLoginByEmail([FromBody] VerifyLoginByEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return Ok(new { result.AccessToken });
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

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return Ok(new { result.AccessToken });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        CancellationToken cancellationToken = default)
    {
        var refreshToken = GetRefreshTokenCookieValue();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is required.");
        }

        var result = await mediator.Send(new RotateRefreshTokenCommand { RefreshToken = refreshToken },
            cancellationToken);

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);

        return Ok(new { result.AccessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var refreshToken = GetRefreshTokenCookieValue();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return NoContent();
        }

        await mediator.Send(new LogoutCommand { RefreshToken = refreshToken }, cancellationToken);

        RemoveRefreshTokenCookie();

        return NoContent();
    }

    private string? GetRefreshTokenCookieValue()
    {
        return Request.Cookies[RefreshTokenCookieName];
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
        });
    }

    private void RemoveRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName);
    }
}