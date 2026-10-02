using Microsoft.OpenApi;
using MyProject.WebApi.Application.Abstractions.Authentication;
using MyProject.WebApi.Presentation.Authentication;
using MyProject.WebApi.Presentation.Middlewares;

namespace MyProject.WebApi.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationLayer(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        services.AddProblemDetails();

        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers();

        services.AddSwaggerDocumentation();
        services.AddEndpointsApiExplorer();
    }

    private static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT token only (without 'Bearer ')"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });
    }
}