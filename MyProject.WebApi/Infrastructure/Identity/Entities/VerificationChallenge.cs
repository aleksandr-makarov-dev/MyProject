namespace MyProject.WebApi.Infrastructure.Identity.Entities;

public sealed class VerificationChallenge
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public ApplicationUser? User { get; init; }
    public required string Purpose { get; init; }
    public required string CodeHash { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public DateTime? UsedAtUtc { get; set; }
    public int Attempts { get; set; }
    public DateTime CreatedAtUtc { get; init; }
}