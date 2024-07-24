using System.ComponentModel;

namespace CoreEntities.RequestModel;

public class CreateStudentRerquest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Họ
    /// </summary>
    [DisplayName("Họ")]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Tên 
    /// </summary>
    [DisplayName("Tên")]
    
    public string? LastName { get; set; }
    
    /// <summary>
    /// Số điện thoại
    /// </summary>
    [DisplayName("Số điện thoại")]
    
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Địa chỉ
    /// </summary>
    [DisplayName("Địa chỉ")]
    
    public string? Address { get; set; }
    
    /// <summary>
    /// Email
    /// </summary>
    [DisplayName("Email")]
    
    public string? Email { get; set; }
    
    /// <summary>
    /// Giới tính
    /// </summary>
    [DisplayName("Giới tính")]

    public bool Gender { get; set; } = true;
    
    /// <summary>
    /// Ngày sinh
    /// </summary>
    [DisplayName("Ngày sinh")]
    
    public DateTime? BirthDay { get; set; }
    

    /// <summary>
    /// Ngày sinh
    /// </summary>
    [DisplayName("Học lực và hạnh kiểm")]
    public Guid ConductId { get; set; } = Guid.Empty;


    [DisplayName("Lớp học")]
    public Guid ClassRoomId { get; set; } = Guid.Empty;
    

    [DisplayName("Phụ huynh")]

    public Guid ParentID { get; set; } = Guid.Empty;
}