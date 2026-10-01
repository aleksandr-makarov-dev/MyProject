namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class VerificationCodeOptions
{
    public const string SectionName = "Authentication:VerificationCode";

    public required string SecretKey { get; init; }
}