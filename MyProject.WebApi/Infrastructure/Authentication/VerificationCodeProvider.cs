using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MyProject.WebApi.Application.Abstractions.Authentication;

namespace MyProject.WebApi.Infrastructure.Authentication;

public sealed class VerificationCodeProvider(IOptions<VerificationCodeOptions> options) : IVerificationCodeProvider
{
    public string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }

    public string ComputeHash(string verificationChallengeId, string userId, string purpose, string code)
    {
        var payload = $"{verificationChallengeId}|{userId}|{purpose}|{code}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.Value.SecretKey));

        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    }

    public bool VerifyCode(string verificationChallengeId, string userId, string purpose, string code,
        string expectedHash)
    {
        var actualHash = ComputeHash(verificationChallengeId, userId, purpose, code);

        var actualHashBytes = Convert.FromBase64String(actualHash);
        var expectedHashBytes = Convert.FromBase64String(expectedHash);

        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }
}