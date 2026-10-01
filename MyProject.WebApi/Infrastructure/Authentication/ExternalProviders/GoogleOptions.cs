namespace MyProject.WebApi.Infrastructure.Authentication.ExternalProviders;

public sealed class GoogleOptions
{
    public const string SectionName = "Authentication:ExternalProviders:Google";

    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}