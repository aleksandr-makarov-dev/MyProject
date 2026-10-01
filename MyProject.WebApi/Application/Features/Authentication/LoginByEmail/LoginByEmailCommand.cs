using Mediator;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByEmail;

public record LoginByEmailCommand : IRequest
{
    public required string Email { get; init; }
}