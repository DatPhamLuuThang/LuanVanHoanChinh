using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Subject : CoreModel<Guid>
{
    /// <summary>
    /// Tên môn học
    /// </summary>
    [DisplayName("Tên môn học")]
    public string? NameSubject { get; set; }
    

    //khóa ngoại
    /// <summary>
    /// Gióa viên dạy môn học
    /// </summary>
    [DisplayName("Giáo viên dạy")]
    public Guid? TeacherId { get; set; }
    public virtual Teacher? Teacher { get; set; }
    
    //tham chiếu
    public virtual ICollection<Transcript>? Transcripts { get; set; }
    public virtual ICollection<Schedule>? Schedules { get; set; }
}