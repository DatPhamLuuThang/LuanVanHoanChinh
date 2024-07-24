namespace SchoolManagement.Models;

public class ScheduleViewModel
{
    public DateTime Day { get; set; }
    public string ClassRoomName { get; set; }
    public string SubjectName { get; set; }
    public string SemesterName { get; set; }
    public int LessonName { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    
    public List<StudentWithTranscript> Students { get; set; }
    
    public Guid SemesterId { get; set; }
    public Guid SubjectId { get; set; }

 
}