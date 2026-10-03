using Microsoft.AspNetCore.Authorization;

namespace MyProject.WebApi.Infrastructure.Identity;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute($"{CustomClaimTypes.Permission}:{permission}");