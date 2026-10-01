using MyProject.WebApi.Infrastructure.Identity;

namespace MyProject.WebApi.Common.Abstractions;

public interface ITokenProvider
{
    string GetAccessToken(ApplicationUser user);
    string GetRefreshToken();
}