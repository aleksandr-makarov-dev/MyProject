using System.Security.Claims;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MyProject.WebApi.Presentation.Authentication;

public sealed class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ??
                                    throw new UnauthorizedException("User not authenticated.");

    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    public Guid UserId => !Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
        ? throw new UnauthorizedException("User is not authenticated.")
        : id;
}