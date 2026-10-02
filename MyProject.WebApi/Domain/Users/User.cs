using Microsoft.AspNetCore.Identity;

namespace MyProject.WebApi.Domain.Users;

public sealed class User : IdentityUser<Guid>, IEntity
{
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<VerificationChallenge> VerificationChallenges { get; set; } = [];
}