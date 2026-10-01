using MyProject.WebApi.Infrastructure.Identity;

namespace MyProject.WebApi.Application.Abstract.Interfaces;

public interface ITokenProvider
{
    string GetAccessToken(ApplicationUser user);
    string GetRefreshToken();
}