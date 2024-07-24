using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Areas.Identity.Models;

public class ResetUserPasswordViewModel
{
    [Required(ErrorMessage = "Id Người dùng là bắt buộc.")]
    public string UserId { get; set; }
    

    public string DisplayNewPassword { get; set; }
}