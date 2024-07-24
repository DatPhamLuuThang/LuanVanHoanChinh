using CoreEntities.SchoolMgntModel;

namespace SchoolManagement.Services.Interface;

public interface IMenuServices
{
    IEnumerable<Menu> LoadMenu(string area);
}