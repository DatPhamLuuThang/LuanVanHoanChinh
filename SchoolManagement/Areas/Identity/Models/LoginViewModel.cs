using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Areas.Identity.Models;

public class LoginViewModel
{
    public string ReturnUrl { get; set; }
    
    [Microsoft.Build.Framework.Required]
    [DataType("Email")]
    public string Username { get; set; }

    [Microsoft.Build.Framework.Required]
    [DataType("Password")]
    public string Password { get; set; }
}