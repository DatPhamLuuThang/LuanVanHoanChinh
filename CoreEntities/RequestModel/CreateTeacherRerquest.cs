using System.ComponentModel;

namespace CoreEntities.RequestModel;

public class CreateTeacherRerquest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    

    [DisplayName("Họ")]
    public string? FirstName { get; set; }
    

    [DisplayName("Tên")]
    
    public string? LastName { get; set; }
    

    [DisplayName("Số điện thoại")]
    
    public string? PhoneNumber { get; set; }
    

    [DisplayName("Địa chỉ")]
    
    public string? Address { get; set; }
    

    [DisplayName("Email")]
    
    public string? Email { get; set; }
    

    [DisplayName("Giới tính")]

    public bool Gender { get; set; } = true;
    

    [DisplayName("Ngày sinh")]
    
    public DateTime? BirthDay { get; set; }
    
    

    [DisplayName("Chuyên môn")]
    public string? Specialize { get; set; }

    [DisplayName("Trình độ")]
    public string? Qualification { get; set; }   

    [DisplayName("Kinh nghiệm")]
    public string? Experience { get; set; }
}