using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;
public class ScheduleTime : CoreModel<Guid>
{
    /// <summary>
    /// Tiết học trong ngày
    /// </summary>
    [DisplayName("Tiết học")]
    public int LessonName { get; set; }
    
    /// <summary>
    /// Thời gian bắt đầu
    /// </summary>
    [DisplayName("Bắt đầu")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }
    
    /// <summary>
    /// Thời gian kết thúc
    /// </summary>
    [DisplayName("Kết thúc")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }
    
    
    public virtual ICollection<Schedule>? Schedules { get; set; }
}       