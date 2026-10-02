namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed record VerifyLoginByEmailResponse
{
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
}