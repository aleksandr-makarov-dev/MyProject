namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "Authentication:RefreshToken";

    public TimeSpan Expiration { get; init; }
}