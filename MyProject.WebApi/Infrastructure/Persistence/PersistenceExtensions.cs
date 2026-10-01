using Microsoft.EntityFrameworkCore;
using MyProject.WebApi.Application.Abstractions;

namespace MyProject.WebApi.Infrastructure.Persistence;

public static class PersistenceExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
    }
}