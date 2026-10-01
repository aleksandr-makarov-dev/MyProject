using Microsoft.AspNetCore.Identity;

namespace MyProject.WebApi.Infrastructure.Identity.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    private ApplicationRole()
    {
    }

    public ApplicationRole(string name) : base(name)
    {
    }
}