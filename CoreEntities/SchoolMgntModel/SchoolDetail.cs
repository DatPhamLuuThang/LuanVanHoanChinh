using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class SchoolDetail : CoreModel<Guid>
{
    /// <summary>
    /// Tên trường
    /// </summary>
    [DisplayName("Tên trường")]
    public string? SchoolName { get; set; }
    /// <summary>
    /// Email trường
    /// </summary>
    [DisplayName("Email trường")]
    [EmailAddress(ErrorMessage = "Sai định dạng Email")]
    public string? Email { get; set; }
    /// <summary>
    /// Số điện thoại trường
    /// </summary>
    [DisplayName("Số điện thoại trường")]
    public string? Phone { get; set; }
    /// <summary>
    /// Địa chỉ trường
    /// </summary>
    [DisplayName("Địa chỉ trường")]
    public string? Adress { get; set; }
    /// <summary>
    /// Trang web hoạt động của trường
    /// </summary>
    [DisplayName("Trang web của trường")]
    public string? Website { get; set; }
    
    //khóa ngoại
    
    
    
    //tham chiếu
    public virtual ICollection<ClassRoom>? ClassRooms { get; set; }
    
}