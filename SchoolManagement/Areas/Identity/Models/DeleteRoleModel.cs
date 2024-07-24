using SchoolManagement.Data.Identity;

namespace SchoolManagement.Areas.Identity.Models;

public class DeleteRoleModel
{
    public User User { get; set; }
    public List<Role> listRole { get; set; }
    
}