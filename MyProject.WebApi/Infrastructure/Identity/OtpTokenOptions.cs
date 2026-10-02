namespace MyProject.WebApi.Infrastructure.Identity;

public sealed class OtpTokenOptions
{
    public const string SectionName = "Authentication:OtpToken";

    public int MaxAttempts { get; set; } = 5;
    public TimeSpan Expires { get; set; } = TimeSpan.FromMinutes(30);
}