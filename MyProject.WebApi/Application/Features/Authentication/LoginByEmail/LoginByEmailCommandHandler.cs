using Mediator;
using Microsoft.AspNetCore.Identity;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Identity;
using UnauthorizedAccessException = MyProject.WebApi.Application.Exceptions.UnauthorizedAccessException;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByEmail;

public sealed class LoginByEmailCommandHandler(
    UserManager<User> userManager,
    ILogger<LoginByEmailCommandHandler> logger)
    : IRequestHandler<LoginByEmailCommand>
{
    public async ValueTask<Unit> Handle(LoginByEmailCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
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
                var error = addToRoleResult.Errors.FirstOrDefault();
                throw new UnauthorizedAccessException(error?.Description ?? "Failed to add user to role.");
            }

            existingUser = user;
        }

        var code = await userManager.GenerateUserTokenAsync(existingUser, TokenProviders.Otp,
            VerificationChallengePurposes.VerifyEmail);

        logger.LogInformation("Verification challenge code: {VerificationChallengeCode}", code);

        return Unit.Value;
    }
}