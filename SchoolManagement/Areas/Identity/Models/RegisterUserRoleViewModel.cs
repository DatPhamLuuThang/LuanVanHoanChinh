using Microsoft.Build.Framework;

namespace SchoolManagement.Areas.Identity.Models
{
    public class RegisterUserRoleViewModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RoleId { get; set; }
    }
}
