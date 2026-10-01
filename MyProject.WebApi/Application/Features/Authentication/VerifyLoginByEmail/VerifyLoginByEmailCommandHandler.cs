using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Infrastructure.Identity;
using MyProject.WebApi.Infrastructure.Identity.Entities;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed class VerifyLoginByEmailCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IVerificationCodeProvider verificationCodeProvider,
    TimeProvider timeProvider,
    ILogger<VerifyLoginByEmailCommandHandler> logger) : IRequestHandler<VerifyLoginByEmailCommand, Guid>
{
    public async ValueTask<Guid> Handle(VerifyLoginByEmailCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
        {
            logger.LogInformation("User with email {Email} not found", request.Email);
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        const string purpose = VerificationChallengePurposes.VerifyEmail;

        // TODO: we should guarantee only one active verification challenge!!!
        var verificationChallenge = await dbContext.VerificationChallenges
            .SingleOrDefaultAsync(x =>
                x.UserId == existingUser.Id &&
                x.Purpose == purpose &&
                x.UsedAtUtc == null, cancellationToken);


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

        if (verificationChallenge.Attempts >= 5)
        {
            logger.LogInformation("Verification challenge {ChallengeId} exceeded maximum attempts.",
                verificationChallenge.Id);
            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        var validationResult = verificationCodeProvider.VerifyCode(verificationChallenge.Id, existingUser.Id, purpose,
            request.Code, verificationChallenge.CodeHash);

        if (!validationResult)
        {
            // Increase number of invalid attempts
            verificationChallenge.Attempts++;
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Invalid verification code for challenge {ChallengeId}. Attempt {Attempts}.",
                verificationChallenge.Id, verificationChallenge.Attempts);

            throw new UnauthorizedAccessException("Invalid email address or code.");
        }

        existingUser.EmailConfirmed = true;
        // Mark verification challenge as used.
        verificationChallenge.UsedAtUtc = utcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        // TODO: create refresh and access token

        // TODO: return usedId from testing purposes
        return existingUser.Id;
    }
}