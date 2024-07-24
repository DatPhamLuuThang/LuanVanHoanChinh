using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Models;

public class HomeIndexViewModel 
{
    public List<SchoolDetail> SchoolDetails { get; set; }
    public List<News> News { get; set; }
    public List<string> SchoolImageFilePaths { get; set; }
    
    public Teacher Teacher { get; set; }
    
public Student Student { get; set; }
    
    public List<ClassRoom> ClassRooms { get; set; }


}