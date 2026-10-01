using MyProject.WebApi.Infrastructure.Authentication;
using MyProject.WebApi.Infrastructure.Identity;
using MyProject.WebApi.Infrastructure.Persistence;

namespace MyProject.WebApi.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistenceLayer(configuration);
        services.AddIdentityLayer();
        services.AddAuthenticationLayer(configuration);
    }
}