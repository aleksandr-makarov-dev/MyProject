namespace MyProject.WebApi.Application.Features.Authentication.LoginByExternalProvider;

public sealed record LoginByExternalProviderResponse
{
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
};