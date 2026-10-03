using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MyProject.WebApi.Infrastructure.Identity;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        const string prefix = $"{CustomClaimTypes.Permission}:";

        if (!policyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var permission = policyName[prefix.Length..];

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();
    }
}