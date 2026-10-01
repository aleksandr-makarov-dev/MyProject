using Mediator;
using Microsoft.AspNetCore.Identity;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Application.Authorization;
using MyProject.WebApi.Infrastructure.Identity;
using MyProject.WebApi.Infrastructure.Identity.Entities;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByEmail;

public sealed class LoginByEmailCommandHandler(
    UserManager<ApplicationUser> userManager,
    IVerificationCodeProvider verificationCodeProvider,
    IApplicationDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<LoginByEmailCommandHandler> logger)
    : IRequestHandler<LoginByEmailCommand>
{
    public async ValueTask<Unit> Handle(LoginByEmailCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
        {
            existingUser = await CreateUserAsync(request.Email);
        }

        var verificationChallengeId = Guid.NewGuid();
        const string verificationChallengePurpose = VerificationChallengePurposes.VerifyEmail;
        var code = verificationCodeProvider.GenerateCode();
        var codeHash = verificationCodeProvider.ComputeHash(verificationChallengeId, existingUser.Id,
            verificationChallengePurpose, code);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var verificationChallenge = new VerificationChallenge
        {
            Id = verificationChallengeId,
            UserId = existingUser.Id,
            Purpose = verificationChallengePurpose,
            CodeHash = codeHash,
            Attempts = 0,
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = utcNow.Add(TimeSpan.FromMinutes(30))
        };

        dbContext.VerificationChallenges.Add(verificationChallenge);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Verification challenge code:{VerificationChallengeCode}", code);

        return Unit.Value;
    }

    private async Task<ApplicationUser> CreateUserAsync(string email)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var createUserResult = await userManager.CreateAsync(user);

        if (!createUserResult.Succeeded)
        {
            var error = createUserResult.Errors.FirstOrDefault();
            throw new UnauthorizedAccessException(error?.Description ?? "Failed to create user.");
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.User);

        if (!addToRoleResult.Succeeded)
        {
            var error = createUserResult.Errors.FirstOrDefault();
            throw new UnauthorizedAccessException(error?.Description ?? "Failed to add user to role.");
        }

        return user;
    }
}