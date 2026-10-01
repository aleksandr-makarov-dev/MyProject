using MyProject.WebApi.Presentation.Middlewares;

namespace MyProject.WebApi.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationLayer(this IServiceCollection services)
    {
        services.AddProblemDetails();
        
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}