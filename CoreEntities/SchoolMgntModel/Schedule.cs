using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Schedule : CoreModel<Guid>
{
    /// <summary>
    /// Ngày học
    /// </summary>
    [DisplayName("Ngày tiết học")]
    [DataType(DataType.Date)]
    [Column(TypeName = "Date")]
    public DateTime Day { get; set; }
    
    //khóa ngoại
    [DisplayName("Tên Lớp học")]
    public Guid ClassRoomId { get; set; }
    [DisplayName("Học kì")]
    public Guid SemesterId { get; set; }
    [DisplayName("Môn học")]
    public Guid SubjectId { get; set; }
    
    [DisplayName("Tiết học")]
    public Guid? ScheduleTimeId { get; set; }
    

    public virtual ScheduleTime? ScheduleTime { get; set; }
    public virtual Semester? Semester { get; set; }
    public virtual Subject? Subject { get; set; }
    public virtual ClassRoom? ClassRoom { get; set; }


    //tham chiếu
}
