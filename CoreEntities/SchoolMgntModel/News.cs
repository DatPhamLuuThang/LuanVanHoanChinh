using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using CoreModels;
using Microsoft.AspNetCore.Http;

namespace CoreEntities.SchoolMgntModel;

public class News: CoreModel<Guid>
{
    /// <summary>
    /// Tin tức hoạt động của trường
    /// </summary>
    [DisplayName("Tin tức của trường")]
    public string? NewsSchool { get; set; }
    
    /// <summary>
    /// Thông báo của trường
    /// </summary>
    [DisplayName("Thông báo của trường")]
    public string? Noty { get; set; }
    
    
    /// <summary>
    /// Hình ảnh của trường
    /// </summary>
    [DisplayName("Hình ảnh của trường")]
    public string? ImgSchoolFileName { get; set; }
    
    [NotMapped] 
    public IFormFile? ImgSchoolFile { get; set; }


}