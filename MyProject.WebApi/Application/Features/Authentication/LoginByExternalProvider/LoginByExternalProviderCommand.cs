using Mediator;

namespace MyProject.WebApi.Application.Features.Authentication.LoginByExternalProvider;

public sealed record LoginByExternalProviderCommand : IRequest<LoginByExternalProviderResponse>
{
    public required string Provider { get; init; }
    public required string ProviderKey { get; init; }
    public string? ProviderDisplayName { get; init; }
    public required string Email { get; init; }
}