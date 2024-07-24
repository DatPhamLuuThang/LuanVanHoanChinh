using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Data.Identity;

public class Role : IdentityRole
{
    public Role() :base(){ }

    public Role(string roleName) : base(roleName)
    {
    }
}