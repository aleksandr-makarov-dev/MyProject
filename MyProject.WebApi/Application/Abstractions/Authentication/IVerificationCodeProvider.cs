namespace MyProject.WebApi.Application.Abstractions.Authentication;

public interface IVerificationCodeProvider
{
    string GenerateCode();
    string ComputeHash(Guid verificationChallengeId, Guid userId, string purpose, string code);
    bool VerifyCode(Guid verificationChallengeId, Guid userId, string purpose, string code, string expectedHash);
}