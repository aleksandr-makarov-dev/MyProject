namespace MyProject.WebApi.Application.Abstractions.Authentication;

public interface ICurrentUserProvider
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
}