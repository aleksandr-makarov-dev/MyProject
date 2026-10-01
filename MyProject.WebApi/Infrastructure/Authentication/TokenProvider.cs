using MyProject.WebApi.Application.Abstractions.Authentication;

namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class TokenProvider : ITokenProvider
{
    public string GetAccessToken(TokenSubject subject)
    {
        throw new NotImplementedException();
    }

    public string GetRefreshToken()
    {
        throw new NotImplementedException();
    }
}