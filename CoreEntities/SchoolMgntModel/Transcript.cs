using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Transcript : CoreModel<Guid>
{
    
    /// <summary>
    /// Tháng của điểm
    /// </summary>
    [DisplayName("Tháng")]
    public int? Month { get; set; }
    
    /// <summary>
    /// Điểm
    /// </summary>
    [DisplayName("Điểm")]
    public decimal Value { get; set; } 
   
    /// <summary>
    /// Loại điểm
    /// </summary>
    [DisplayName("Loại điểm")]
    public int TypeId { get; set; }
    
    
    
    //khóa ngoại
    /// <summary>
    /// Học sinh
    /// </summary>
    [DisplayName("Học sinh")]
    public Guid StudentId { get; set; }
    public virtual Student Student { get; set; } 
    
    /// <summary>
    /// Môn học
    /// </summary>
    [DisplayName("Môn học")]
    public Guid SubjectId { get; set; }
    public virtual Subject Subject { get; set; }
    
    /// <summary>
    /// Học kì
    /// </summary>
    [DisplayName("Học kì")]
    public Guid SemesterId { get; set; }
    public virtual Semester Semester { get; set; }
    
}