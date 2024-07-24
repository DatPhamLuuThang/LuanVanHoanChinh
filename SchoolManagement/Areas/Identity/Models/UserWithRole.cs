
using SchoolManagement.Data.Identity;

namespace SchoolManagement.Areas.Identity.Models;

public class UserWithRole
{
    public User User { get; set; }
    public List<Role> ListRole { get; set; }
    
}