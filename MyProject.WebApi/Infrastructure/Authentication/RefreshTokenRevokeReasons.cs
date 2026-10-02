namespace MyProject.WebApi.Infrastructure.Authentication;

public static class RefreshTokenRevokeReasons
{
    public const string Logout = "logout";
    public const string Rotation = "token_rotation";
    public const string ReuseDetected = "token_reuse_detected";
}