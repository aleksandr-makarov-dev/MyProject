namespace MyProject.WebApi.Application.Abstractions.Authentication;

public sealed record TokenSubject
{
    public Guid UserId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}