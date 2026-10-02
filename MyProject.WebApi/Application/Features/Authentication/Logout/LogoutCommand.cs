using Mediator;

namespace MyProject.WebApi.Application.Features.Authentication.Logout;

public sealed record LogoutCommand : IRequest<Unit>
{
    public required string RefreshToken { get; init; }
};