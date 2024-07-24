using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Semester : CoreModel<Guid>
{
    /// <summary>
    /// Tên học kì
    /// </summary>
    [DisplayName("Tên học kì")]
    public string? NameSemester { get; set; }
    /// <summary>
    /// Học kì bắt đầu vào
    /// </summary>
    [DisplayName("Bắt đầu từ")]
    [DataType(DataType.Date)]
    public DateTime? StartIn { get; set; }
    /// <summary>
    /// Học kì kết thúc vào
    /// </summary>
    [DisplayName("Kết thúc vào")]
    [DataType(DataType.Date)]
    public DateTime? EndIn { get; set; }
    
    [DisplayName("Tuần thi")]
    public int ExamWeek { get; set; }
    
    /// <summary>
    /// Học kì kết thúc vào
    /// </summary>
    [DisplayName("Học phí")]
   
    public int? Tuition { get; set; }
    
    //khóa ngoại
    
    
    //tham chiếu
    public virtual ICollection<Transcript>? Transcripts { get; set; }
    public virtual ICollection<Schedule>? Schedules { get; set; }
    

    
}