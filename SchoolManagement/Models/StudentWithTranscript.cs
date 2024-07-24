using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class StudentWithTranscript 
{
    public Student Students { get; set; }
    
    public Guid StudentId { get; set; }
    
    public List<Transcript> Transcripts { get; set; }
    


    
}