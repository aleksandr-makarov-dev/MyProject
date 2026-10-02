using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;

namespace MyProject.WebApi.Infrastructure.Identity;

public sealed class OtpTokenProvider<TUser>(
    IApplicationDbContext dbContext,
    IVerificationCodeProvider verificationCodeProvider,
    IOptions<OtpTokenProviderOptions> options,
    TimeProvider timeProvider)
    : IUserTwoFactorTokenProvider<TUser> where TUser : class
{
    public async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var userIdString = await manager.GetUserIdAsync(user);
        var userId = Guid.Parse(userIdString);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var challengeId = Guid.NewGuid();

        var code = verificationCodeProvider.GenerateCode();
        var codeHash = verificationCodeProvider.ComputeHash(challengeId.ToString(), userIdString, purpose, code);

        await using var transaction =
            await dbContext.BeginTransactionAsync();

        try
        {
            await dbContext.VerificationChallenges
                .Where(x =>
                    x.UserId == userId &&
                    x.Purpose == purpose &&
                    x.UsedAtUtc == null &&
                    x.RevokedAtUtc == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAtUtc, utcNow));

            var verificationChallenge = new VerificationChallenge
            {
                Id = challengeId,
                UserId = userId,
                Purpose = purpose,
                CodeHash = codeHash,
                Attempts = 0,
                CreatedAtUtc = utcNow,
                ExpiresAtUtc = utcNow.Add(options.Value.Expiration),
            };

            dbContext.VerificationChallenges.Add(verificationChallenge);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return code;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        var userIdString = await manager.GetUserIdAsync(user);
        var userId = Guid.Parse(userIdString);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var verificationChallenge = await dbContext.VerificationChallenges
            .SingleOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Purpose == purpose &&
                x.UsedAtUtc == null &&
                x.RevokedAtUtc == null);

        if (verificationChallenge is null)
        {
            return false;
        }

        if (verificationChallenge.Attempts >= options.Value.MaxAttempts)
        {
            return false;
        }

        if (verificationChallenge.ExpiresAtUtc <= utcNow)
        {
            return false;
        }

        var isValid = verificationCodeProvider.VerifyCode(verificationChallenge.Id.ToString(), userIdString, purpose,
            token, verificationChallenge.CodeHash);

        if (!isValid)
        {
            verificationChallenge.Attempts++;
            await dbContext.SaveChangesAsync();

            return false;
        }

        verificationChallenge.UsedAtUtc = utcNow;
        await dbContext.SaveChangesAsync();

        return true;
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user) => Task.FromResult(true);
}