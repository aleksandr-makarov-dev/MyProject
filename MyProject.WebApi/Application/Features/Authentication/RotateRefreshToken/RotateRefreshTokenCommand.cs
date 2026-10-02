using Mediator;

namespace MyProject.WebApi.Application.Features.Authentication.RotateRefreshToken;

public sealed record RotateRefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public required string RefreshToken { get; set; }
};