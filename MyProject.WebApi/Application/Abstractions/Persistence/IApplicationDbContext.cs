using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyProject.WebApi.Domain.Users;

namespace MyProject.WebApi.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<VerificationChallenge> VerificationChallenges { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}