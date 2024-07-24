using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Areas.Identity.Models;

public class ChangePassword
{
   public string ReturnUrl { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string OldPassword { get; set; }
    
    [Required]
    [StringLength(100)]
    [MinLength(10)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "Nhập lại mật khẩu không chính xác.")]
    public string ConfirmPassword { get; set; }
    
    
}