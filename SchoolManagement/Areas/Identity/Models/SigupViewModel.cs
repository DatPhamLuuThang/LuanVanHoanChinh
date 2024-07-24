using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SchoolManagement.Areas.Identity.Models;

public class SigupViewModel
{
    [NotMapped]
    public string? ReturnUrl { get; set; }
    
    [Required]
    [DataType("Email")]
    public string Username { get; set; }
    
    [Required]
    [DataType("Password")]
    public string Password { get; set; }
    [Required]
    [Compare("Password")]
    [DataType("Password")]
    public string ConfirmPassword { get; set; }
    
    public bool IsTeacher { get; set; }
}