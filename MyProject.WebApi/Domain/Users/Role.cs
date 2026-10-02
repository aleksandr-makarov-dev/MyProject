using Microsoft.AspNetCore.Identity;

namespace MyProject.WebApi.Domain.Users;

public sealed class Role : IdentityRole<Guid>, IEntity
{
    private Role()
    {
    }

    public Role(string name) : base(name)
    {
    }
}