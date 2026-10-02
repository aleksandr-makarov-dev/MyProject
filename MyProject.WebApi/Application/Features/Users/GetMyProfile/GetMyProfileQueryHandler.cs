using Mediator;
using Microsoft.AspNetCore.Identity;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Application.Exceptions;
using MyProject.WebApi.Domain.Users;

namespace MyProject.WebApi.Application.Features.Users.GetMyProfile;

public sealed class GetMyProfileQueryHandler(ICurrentUserProvider currentUserProvider, UserManager<User> userManager)
    : IRequestHandler<GetMyProfileQuery, GetMyProfileResponse>
{
    public async ValueTask<GetMyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUserProvider.UserId.ToString());

        if (user is null)
        {
            throw new UnauthorizedException("User not found.");
        }

        var roles = await userManager.GetRolesAsync(user);

        return new GetMyProfileResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = [.. roles]
        };
    }
}