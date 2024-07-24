using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Violation : CoreModel<Guid>
{
    /// <summary>
    /// Ngày vi phạm
    /// </summary>
    [DisplayName("Ngày vi phạm")]
    public DateTime? DayOfViolate{ get; set; }
    /// <summary>
    /// Lỗi vi phạm
    /// </summary>
    [DisplayName("Lỗi vi phạm")]
    public string? Violate{ get; set; }
    /// <summary>
    /// Số vi phạm
    /// </summary>
    [DisplayName("Số vi phạm")]
    public int? NumberOfViolate{ get; set; }
    
    //khóa ngoại
    [DisplayName("Học sinh")]
    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }

}