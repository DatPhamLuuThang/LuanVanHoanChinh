using System.ComponentModel;
using CoreModels;

namespace CoreEntities.SchoolMgntModel;

public class Menu: CoreModel<Guid>
{
        /// <summary>
        /// Tên Menu
        /// </summary>
        [DisplayName("Tên Menu")]
        public required string Name { get; set; }
        
        /// <summary>
        /// Mô tả của Menu
        /// </summary>
        [DisplayName("Mô tả")]
        public string? Description { get; set; }
        
        /// <summary>
        /// Area
        /// </summary>
        [DisplayName("Area")]
        public string? Area { get; set; }
        /// <summary>
        /// Controller
        /// </summary>
        [DisplayName("Controller")]
        public string? Controller { get; set; }
        /// <summary>
        /// Biểu tượng của menu
        /// </summary>
        [DisplayName("Biểu tượng")]
        public string? Icon { get; set; }
        /// <summary>
        /// Thứ tự của menu
        /// </summary>
        [DisplayName("Thứ tự")]
        public int Order { get; set; }
        /// <summary>
        /// Cấp độ cua Menu
        /// </summary>
        [DisplayName("Cấp độ")]
        public int Level { get; set; }
        
        //Khóa ngoại
        public Guid? ParentId { get; set; }
        
        //Tham chiếu
        public virtual Menu? Parent { get; set; }
        
        //Tham chiếu
        public virtual ICollection<Menu>? Child { get; set; }
}