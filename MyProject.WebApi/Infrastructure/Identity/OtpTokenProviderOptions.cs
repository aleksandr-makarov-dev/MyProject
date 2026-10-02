namespace MyProject.WebApi.Infrastructure.Identity;

public sealed class OtpTokenProviderOptions
{
    public const string SectionName = "Authentication:OtpTokenProvider";

    public int MaxAttempts { get; init; }
    public TimeSpan Expiration { get; init; }
}