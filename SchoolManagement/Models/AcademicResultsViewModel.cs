using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class AcademicResultsViewModel
{
    public Student Student { get; set; }
    public List<Transcript> AcademicResults { get; set; }
    public string Rank { get; set; }
    public string KindofConduct { get; set; }
}