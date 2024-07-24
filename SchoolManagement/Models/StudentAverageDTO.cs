namespace SchoolManagement.Models;

public class StudentAverageDTO
{
    public string StudentName { get; set; }
    public List<Tuple<string, double?>> SubjectAverages { get; set; }
}