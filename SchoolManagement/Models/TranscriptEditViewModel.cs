using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class TranscriptEditViewModel
{
    public Transcript Transcript { get; set; }
    public Student Student { get; set; }
    public List<Transcript> ExistingScores { get; set; }
    public Guid SelectedExistingScore { get; set; }
}

public class TranscriptEditRequestModel
{
    public Guid TranId { get; set; } = Guid.Empty;
    public decimal newValue { get; set; } = 0;
}