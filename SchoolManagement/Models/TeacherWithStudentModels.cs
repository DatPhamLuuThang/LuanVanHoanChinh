using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class TeacherWithStudentModels
{
    public Teacher? Teacher { get; set; }
    public List<ClassRoom> ClassRooms { get; set; }
    public List<Student> Students { get; set; }
    

}