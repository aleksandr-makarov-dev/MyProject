using System.Security.Cryptography;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Authentication;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.RotateRefreshToken;

public sealed class RotateRefreshTokenCommandHandler(
    IApplicationDbContext dbContext,
    UserManager<User> userManager,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    ITokenProvider tokenProvider,
    TimeProvider timeProvider,
    ILogger<RotateRefreshTokenCommandHandler> logger)
    : IRequestHandler<RotateRefreshTokenCommand, RefreshTokenResponse>
{
    public async ValueTask<RefreshTokenResponse> Handle(
        RotateRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var refreshTokenAsBytes = Convert.FromBase64String(request.RefreshToken);
        var tokenHash = Convert.ToHexString(SHA256.HashData(refreshTokenAsBytes));

        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null)
        {
            logger.LogWarning("Refresh token was not found.");
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (refreshToken.IsRevoked)
        {
            logger.LogWarning(
                "Refresh token reuse detected for user {UserId} and token group {GroupId}. Revoking token group.",
                refreshToken.UserId,
                refreshToken.GroupId);

            await dbContext.RefreshTokens
                .Where(x => x.GroupId == refreshToken.GroupId && !x.IsRevoked)
                .ExecuteUpdateAsync(setters =>
                {
                    setters.SetProperty(x => x.IsRevoked, true);
                    setters.SetProperty(x => x.RevokedAtUtc, utcNow);
                    setters.SetProperty(x => x.RevokeReason, "token_reuse_detected");
                    setters.SetProperty(x => x.UpdatedAtUtc, utcNow);
                }, cancellationToken);

            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await userManager.FindByIdAsync(
            refreshToken.UserId.ToString());

        if (refreshToken.ExpiresAtUtc <= utcNow)
        {
            logger.LogWarning("Expired refresh token used by user {UserId}. Token expired at {ExpiresAtUtc}.",
                refreshToken.UserId, refreshToken.ExpiresAtUtc);

            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (user is null)
        {
            logger.LogWarning("User {UserId} associated with refresh token was not found.", refreshToken.UserId);

            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var roles = await userManager.GetRolesAsync(user);

        var newRefreshTokenString = tokenProvider.GetRefreshToken();
        var newRefreshTokenBytes = Convert.FromBase64String(newRefreshTokenString);

        var tokenSubject = new TokenSubject
        {
            UserId = user.Id,
            Roles = [.. roles]
        };

        var newAccessToken = tokenProvider.GetAccessToken(tokenSubject);

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAtUtc = utcNow;
        refreshToken.RevokeReason = "token_refresh";

        var newRefreshToken = new RefreshToken
        {
            UserId = refreshToken.UserId,
            TokenHash = Convert.ToHexString(SHA256.HashData(newRefreshTokenBytes)),
            GroupId = refreshToken.GroupId,
            ExpiresAtUtc = utcNow.Add(refreshTokenOptions.Value.Expiration),
            IsRevoked = false
        };

        dbContext.RefreshTokens.Add(newRefreshToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString
        };
    }
}