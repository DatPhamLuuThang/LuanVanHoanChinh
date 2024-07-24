using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Student : CoreUser<Guid>
{   
    
    /// <summary>
    /// Học lực của học sinh
    /// </summary>
    [DisplayName("Học lực")]
    public string? Rank { get; set; }
    
    
    /// <summary>
    /// Hạnh kiểm của học sinh
    /// </summary>
    [DisplayName("Hạnh kiểm")]
    public string? KindofConduct { get; set; }
    
    
    //khóa ngoại

    [DisplayName("Lớp")]
    public Guid? ClassRoomId { get; set; } 
    public virtual ClassRoom? ClassRoom { get; set; }
    
    
    [DisplayName("Phụ huynh")]
    public Guid? ParentId { get; set; } 
    
    
    //tham chiếu
    public virtual Parents? Parents { get; set; }
    public virtual ICollection<Transcript>? Transcripts { get; set; }
    public virtual ICollection<Violation>? Violations { get; set; }

}