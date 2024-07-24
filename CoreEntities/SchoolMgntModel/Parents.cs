using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Parents:CoreUser<Guid>
{
    //khóa ngoại


    
    
    //tham chiếu
    public virtual ICollection<Student>? Students { get; set; }
}