using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Infrastructure.Identity.Entities;

namespace MyProject.WebApi.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<VerificationChallenge> VerificationChallenges { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}