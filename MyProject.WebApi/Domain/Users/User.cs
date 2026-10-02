using Microsoft.AspNetCore.Identity;

namespace MyProject.WebApi.Domain.Users;

public sealed class User : IdentityUser<Guid>
{
    public ICollection<VerificationChallenge> VerificationChallenges { get; set; } = [];
}