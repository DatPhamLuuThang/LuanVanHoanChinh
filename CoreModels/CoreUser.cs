using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoreModels;

public abstract class CoreUser<TKey> : CoreModel<TKey>
{
    /// <summary>
    /// Tên 
    /// </summary>
    [DisplayName("Tên")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Họ
    /// </summary>
    [DisplayName("Họ")]
    public string? LastName { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    [DisplayName("Số điện thoại")]
    [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Địa chỉ
    /// </summary>
    [DisplayName("Địa chỉ")]
    public string? Address { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    [DisplayName("Email")]
    [EmailAddress(ErrorMessage = "Sai định dạng Email")]
    public string? Email { get; set; }

    /// <summary>
    /// Giới tính
    /// </summary>
    [DisplayName("Giới tính")]
    public bool Gender { get; set; }

    /// <summary>
    /// Ngày tháng năm sinh
    /// </summary>
    [DisplayName("Ngày tháng năm sinh")]
    [DataType(DataType.Date)]
    // [Range(typeof(DateTime), "1/1/1900", "1/1/2007", ErrorMessage = "Bạn phải từ 15 tuổi trở lên.")]
    [MinimumAge(15, ErrorMessage = "Bạn phải từ 15 tuổi trở lên.")]
    public DateTime BirthDay { get; set; }
    
}

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (DateTime.Today.Year - date.Year < _minimumAge)
            {
                return new ValidationResult($"Bạn phải từ {_minimumAge} tuổi trở lên.");
            }
        }

        return ValidationResult.Success;
    }
}