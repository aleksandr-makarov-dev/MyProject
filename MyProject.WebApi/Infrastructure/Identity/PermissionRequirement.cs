using Microsoft.AspNetCore.Authorization;

namespace MyProject.WebApi.Infrastructure.Identity;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;