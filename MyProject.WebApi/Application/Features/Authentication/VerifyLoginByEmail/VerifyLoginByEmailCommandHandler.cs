using System.Security.Cryptography;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Authentication;
using MyProject.WebApi.Infrastructure.Identity;
using UnauthorizedAccessException =
    MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed class VerifyLoginByEmailCommandHandler(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    ITokenProvider tokenProvider,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    TimeProvider timeProvider,
    ILogger<VerifyLoginByEmailCommandHandler> logger)
    : IRequestHandler<VerifyLoginByEmailCommand, VerifyLoginByEmailResponse>
{
    public async ValueTask<VerifyLoginByEmailResponse> Handle(
        VerifyLoginByEmailCommand request,
        CancellationToken cancellationToken)

    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
        {
            logger.LogInformation("Login verification failed because user was not found.");
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        var isValid = await userManager.VerifyUserTokenAsync(existingUser, TokenProviders.Otp,
            VerificationChallengePurposes.VerifyEmail, request.Code);

        if (!isValid)
        {
            logger.LogInformation("Login verification failed because code was invalid.");
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        var userRoles = await userManager.GetRolesAsync(existingUser);

        var refreshTokenString = tokenProvider.GetRefreshToken();
        var refreshTokenBytes = Convert.FromBase64String(refreshTokenString);

        var tokenSubject = new TokenSubject
        {
            UserId = existingUser.Id,
            Roles = [.. userRoles]
        };

        var accessToken = tokenProvider.GetAccessToken(tokenSubject);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        existingUser.EmailConfirmed = true;

        var refreshToken = new RefreshToken
        {
            UserId = existingUser.Id,
            TokenHash = Convert.ToHexString(SHA256.HashData(refreshTokenBytes)),
            GroupId = Guid.NewGuid(),
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow.Add(refreshTokenOptions.Value.Expiration),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(refreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new VerifyLoginByEmailResponse
        {
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            AccessToken = accessToken
        };
    }
}