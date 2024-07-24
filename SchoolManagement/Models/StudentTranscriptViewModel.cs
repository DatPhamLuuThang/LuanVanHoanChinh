using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class StudentTranscriptViewModel
{
    public Teacher Teacher { get; set; }
    public Student Student { get; set; }
}