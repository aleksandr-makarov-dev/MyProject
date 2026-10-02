using System.Security.Cryptography;
using Mediator;
using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Infrastructure.Authentication;

namespace MyProject.WebApi.Application.Features.Authentication.Logout;

public sealed class LogoutCommandHandler(IApplicationDbContext dbContext, TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand>
{
    public async ValueTask<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenAsBytes = Convert.FromBase64String(request.RefreshToken);
        var tokenHash = Convert.ToHexString(SHA256.HashData(refreshTokenAsBytes));

        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null)
        {
            return Unit.Value;
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        await dbContext.RefreshTokens
            .Where(x =>
                x.GroupId == refreshToken.GroupId &&
                !x.IsRevoked)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsRevoked, true)
                    .SetProperty(x => x.RevokedAtUtc, utcNow)
                    .SetProperty(x => x.RevokeReason, RefreshTokenRevokeReasons.Logout)
                    .SetProperty(x => x.UpdatedAtUtc, utcNow),
                cancellationToken);

        return Unit.Value;
    }
}