using System.Security.Cryptography;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Application.Exceptions;
using MyProject.WebApi.Domain.Users;
using MyProject.WebApi.Infrastructure.Authentication;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByExternalProvider;

public sealed class
    LoginByExternalProviderCommandHandler(
        UserManager<User> userManager,
        ITokenProvider tokenProvider,
        TimeProvider timeProvider,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        IApplicationDbContext dbContext,
        ILogger<LoginByExternalProviderCommandHandler> logger)
    : IRequestHandler<LoginByExternalProviderCommand, LoginByExternalProviderResponse>
{
    public async ValueTask<LoginByExternalProviderResponse> Handle(LoginByExternalProviderCommand request,
        CancellationToken cancellationToken)
    {
        var externalLoginUser = await userManager.FindByLoginAsync(request.Provider, request.ProviderKey);

        var existingUser = externalLoginUser ?? await userManager.FindByEmailAsync(request.Email);

        if (existingUser is null)
        {
            logger.LogInformation("No existing user found for external provider {Provider}. Creating a new user.",
                request.Provider);

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(user);

            if (!createUserResult.Succeeded)
            {
                var error = createUserResult.Errors.FirstOrDefault();
                throw new UnauthorizedException(error?.Description ?? "Failed to create user.");
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.User);

            if (!addToRoleResult.Succeeded)
            {
                var error = addToRoleResult.Errors.FirstOrDefault();
                throw new UnauthorizedException(error?.Description ?? "Failed to add user to role.");
            }

            existingUser = user;
        }

        if (externalLoginUser is null)
        {
            logger.LogInformation("Adding external login provider {Provider} to the user.", request.Provider);

            var addLoginResult = await userManager.AddLoginAsync(existingUser,
                new UserLoginInfo(request.Provider, request.ProviderKey, request.ProviderDisplayName));

            if (!addLoginResult.Succeeded)
            {
                var error = addLoginResult.Errors.FirstOrDefault();
                throw new BadRequestException(error?.Description ?? "Failed to add external login.");
            }
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

        return new LoginByExternalProviderResponse
        {
            RefreshToken = refreshTokenString,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            AccessToken = accessToken
        };
    }
}