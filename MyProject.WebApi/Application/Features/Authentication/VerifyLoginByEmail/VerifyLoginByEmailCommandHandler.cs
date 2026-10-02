using System.Security.Cryptography;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Authentication;
using UnauthorizedAccessException =
    MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed class VerifyLoginByEmailCommandHandler(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IVerificationCodeProvider verificationCodeProvider,
    ITokenProvider tokenProvider,
    IOptions<RefreshTokenOptions> refreshTokenOptions,
    TimeProvider timeProvider,
    ILogger<VerifyLoginByEmailCommandHandler> logger)
    : IRequestHandler<VerifyLoginByEmailCommand, VerifyLoginByEmailResponse>
{
    private const int MaxVerificationAttempts = 5;

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

        const string purpose = VerificationChallengePurposes.VerifyEmail;

        var verificationChallenge = await dbContext.VerificationChallenges
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == existingUser.Id &&
                    x.Purpose == purpose &&
                    x.UsedAtUtc == null,
                cancellationToken);

        if (verificationChallenge is null)
        {
            logger.LogInformation(
                "Active verification challenge for user {UserId} and purpose {Purpose} was not found.", existingUser.Id,
                purpose);
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (verificationChallenge.ExpiresAtUtc <= utcNow)
        {
            logger.LogInformation("Verification challenge {ChallengeId} is expired.", verificationChallenge.Id);
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        if (verificationChallenge.Attempts >= MaxVerificationAttempts)
        {
            logger.LogInformation("Verification challenge {ChallengeId} exceeded maximum attempts.",
                verificationChallenge.Id);
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        var isValidCode = verificationCodeProvider.VerifyCode(verificationChallenge.Id, existingUser.Id, purpose,
            request.Code, verificationChallenge.CodeHash);

        if (!isValidCode)
        {
            var rows = await dbContext.VerificationChallenges
                .Where(x =>
                    x.Id == verificationChallenge.Id &&
                    x.UserId == existingUser.Id &&
                    x.Purpose == purpose &&
                    x.UsedAtUtc == null &&
                    x.ExpiresAtUtc > utcNow &&
                    x.Attempts < MaxVerificationAttempts)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Attempts, x => x.Attempts + 1),
                    cancellationToken);

            if (rows == 1)
            {
                logger.LogInformation("Invalid verification code for challenge {ChallengeId}.",
                    verificationChallenge.Id);
            }
            else
            {
                logger.LogInformation(
                    "Verification challenge {ChallengeId} could not record an attempt because its state changed.",
                    verificationChallenge.Id);
            }

            throw new UnauthorizedAccessException(
                "Invalid email address or code.");
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

        await using var transaction =
            await dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var rows = await dbContext.VerificationChallenges
                .Where(x =>
                    x.Id == verificationChallenge.Id &&
                    x.UserId == existingUser.Id &&
                    x.Purpose == purpose &&
                    x.UsedAtUtc == null &&
                    x.ExpiresAtUtc > utcNow &&
                    x.Attempts < MaxVerificationAttempts)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.UsedAtUtc, utcNow),
                    cancellationToken);

            if (rows != 1)
            {
                logger.LogInformation("Verification challenge {ChallengeId} could not be consumed.",
                    verificationChallenge.Id);
                throw new UnauthorizedAccessException("Invalid email address or code.");
            }

            existingUser.EmailConfirmed = true;

            var refreshToken = new RefreshToken
            {
                UserId = existingUser.Id,
                TokenHash = Convert.ToHexString(SHA256.HashData(refreshTokenBytes)),
                GroupId = Guid.NewGuid(),
                ExpiresAtUtc = utcNow.Add(refreshTokenOptions.Value.Expiration),
                IsRevoked = false
            };

            dbContext.RefreshTokens.Add(refreshToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new VerifyLoginByEmailResponse
        {
            RefreshToken = refreshTokenString,
            AccessToken = accessToken
        };
    }
}