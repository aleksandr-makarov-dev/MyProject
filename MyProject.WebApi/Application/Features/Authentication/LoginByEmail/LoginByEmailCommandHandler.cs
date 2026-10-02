using Mediator;
using Microsoft.AspNetCore.Identity;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Identity;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByEmail;

public sealed class LoginByEmailCommandHandler(
    UserManager<User> userManager,
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

        // TODO: we should guarantee only one active verification challenge!!!
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

    private async Task<User> CreateUserAsync(string email)
    {
        var user = new User
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