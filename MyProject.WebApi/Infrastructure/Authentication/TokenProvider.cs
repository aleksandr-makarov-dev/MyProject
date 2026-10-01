using MyProject.WebApi.Application.Abstractions;
using MyProject.WebApi.Infrastructure.Identity;

namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class TokenProvider : ITokenProvider
{
    public string GetAccessToken(ApplicationUser user)
    {
        throw new NotImplementedException();
    }

    public string GetRefreshToken()
    {
        throw new NotImplementedException();
    }
}