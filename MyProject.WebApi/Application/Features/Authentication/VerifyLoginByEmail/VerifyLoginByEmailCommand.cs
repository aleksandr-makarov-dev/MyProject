using Mediator;

namespace MyProject.WebApi.Application.Features.Authentication.VerifyLoginByEmail;

public sealed record VerifyLoginByEmailCommand : IRequest<VerifyLoginByEmailResponse>
{
    public required string Email { get; init; }
    public required string Code { get; init; }
}