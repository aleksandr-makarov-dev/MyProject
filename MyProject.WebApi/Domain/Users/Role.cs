using Microsoft.AspNetCore.Identity;

namespace MyProject.WebApi.Domain.Users;

public sealed class Role : IdentityRole<Guid>
{
    private Role()
    {
    }

    public Role(string name) : base(name)
    {
    }
}