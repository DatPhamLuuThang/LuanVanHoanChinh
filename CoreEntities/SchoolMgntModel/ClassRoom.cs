using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class ClassRoom: CoreModel<Guid>
{

    /// <summary>
    /// Tên lớp học
    /// </summary>
    [DisplayName("Tên Lớp học")]
    public string? ClassName { get; set; }
    
    /// <summary>
    /// Niên khóa học 
    /// </summary>
    [DisplayName("Niên khóa")]
    public string? SchoolYear { get; set; } 
    
    /// <summary>
    /// Sỉ số học sinh
    /// </summary>
    [DisplayName("Sỉ số")]
    public int? Total { get; set; }
    
    //Khoá ngoại
    public Guid? SchoolDetailId { get; set; }
    public virtual SchoolDetail? SchoolDetail { get; set; }
    
    [DisplayName("Giáo viên chủ nhiệm")]
    public Guid? TeacherId { get; set; }
    public virtual Teacher? Teacher { get; set; }
    
    //tham chiếu
    public virtual ICollection<Schedule>? Schedules { get; set; }
    public virtual ICollection<Student>? Students { get; set; }
}