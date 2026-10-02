using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Application.Abstractions.Persistence;
using MyProject.WebApi.Domain.Users;

namespace MyProject.WebApi.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options), IApplicationDbContext
{
    public DbSet<VerificationChallenge> VerificationChallenges => Set<VerificationChallenge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}