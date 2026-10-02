using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Domain.Users;

namespace MyProject.WebApi.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<VerificationChallenge> VerificationChallenges { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}