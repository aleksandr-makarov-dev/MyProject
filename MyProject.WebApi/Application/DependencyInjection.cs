using FluentValidation;

namespace MyProject.WebApi.Application;

public static class DependencyInjection
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    }
}