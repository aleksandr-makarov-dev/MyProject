namespace MyProject.WebApi.Application.Features.Authentication.RotateRefreshToken;

public sealed record RefreshTokenResponse
{
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
};