using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class ClassStudentsViewModel
{
    public Teacher Teacher { get; set; }
    public ClassRoom ClassRoom { get; set; }
    
    public List<StudentWithTranscript> Students{ get; set; }
    
    public Guid SemesterId { get; set; }
    public Guid SubjectId { get; set; }
}