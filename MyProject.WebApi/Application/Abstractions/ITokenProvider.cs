using MyProject.WebApi.Infrastructure.Identity;

namespace MyProject.WebApi.Application.Abstractions;

public interface ITokenProvider
{
    string GetAccessToken(ApplicationUser user);
    string GetRefreshToken();
}