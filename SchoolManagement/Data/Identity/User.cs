using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Data.Identity;

public class User : IdentityUser
{
    public bool Active { get; set; }
}