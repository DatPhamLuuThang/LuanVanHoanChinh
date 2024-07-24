using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class AllScoresByMonthViewModel
{
    public Teacher Teacher { get; set; }
    public int Month { get; set; }
    public List<StudentWithTranscript> StudentWithTranscripts { get; set; }
}