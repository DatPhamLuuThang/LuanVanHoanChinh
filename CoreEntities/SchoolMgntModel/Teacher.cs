using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Teacher :  CoreUser<Guid>
{
    /// <summary>
    /// Chuyên ngành dạy học
    /// </summary>
    [DisplayName("Chuyên môn")]
    public string? Specialize { get; set; }
    /// <summary>
    /// Trình độ học vấn
    /// </summary>=
    [DisplayName("Trình độ")]
    public string? Qualification { get; set; }   
    /// <summary>
    /// Thâm niên trong nghề    
    /// </summary>
    [DisplayName("Kinh nghiệm")]
    public string? Experience { get; set; }
    
    //khóa ngoại
    
    
    
    //tham chiếu
    public virtual ICollection<ClassRoom>? ClassRooms { get; set; }
    public virtual ICollection<Subject>? Subjects { get; set; }

}