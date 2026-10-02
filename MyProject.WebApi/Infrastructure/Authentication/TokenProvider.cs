using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyProject.WebApi.Application.Abstractions.Authentication;

namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class TokenProvider(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenProvider
{
    public string GetAccessToken(TokenSubject subject)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.UserId.ToString()),
        };

        claims.AddRange(subject.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAtUtc = timeProvider.GetUtcNow().UtcDateTime.Add(options.Value.Expiration);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            SigningCredentials = credentials,
            Issuer = options.Value.Issuer,
            Audience = options.Value.Audience,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public string GetRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}