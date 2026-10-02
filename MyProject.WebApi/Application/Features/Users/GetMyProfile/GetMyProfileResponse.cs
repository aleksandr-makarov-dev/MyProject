namespace MyProject.WebApi.Application.Features.Users.GetMyProfile;

public sealed record GetMyProfileResponse
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public List<string> Roles { get; init; } = [];
};