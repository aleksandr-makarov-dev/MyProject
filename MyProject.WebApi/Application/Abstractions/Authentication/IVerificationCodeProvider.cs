namespace MyProject.WebApi.Application.Abstractions.Authentication;

public interface IVerificationCodeProvider
{
    string GenerateCode();
    string ComputeHash(string verificationChallengeId, string userId, string purpose, string code);
    bool VerifyCode(string verificationChallengeId, string userId, string purpose, string code, string expectedHash);
}