using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class StudentList
{
    public Teacher Teacher { get; set; }
    public List<ClassRoom> ClassRooms { get; set; }
    public List<StudentWithTranscript> Students { get; set; }
}