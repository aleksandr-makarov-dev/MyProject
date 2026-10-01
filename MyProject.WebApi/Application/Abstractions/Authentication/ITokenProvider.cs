namespace MyProject.WebApi.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string GetAccessToken(TokenSubject subject);
    string GetRefreshToken();
}