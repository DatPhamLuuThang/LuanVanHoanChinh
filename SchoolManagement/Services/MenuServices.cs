using CoreEntities.SchoolMgntModel;
using SchoolManagement.Services.Interface;

namespace SchoolManagement.Services;

public class MenuServices : IMenuServices
{
    /* Data Services */
    private readonly ICoreServices<Menu> _menuServices;
    
    public MenuServices(ICoreServices<Menu> menuServices)
    {
        _menuServices = menuServices;
    }

    public IEnumerable<Menu> LoadMenu(string area)
    {
        return string.IsNullOrEmpty(area)?
        _menuServices
            .GetAll()
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList()
            .Where(x => x.Level == 0)
        :_menuServices
            .GetAll()
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList()
            .Where(x => x.Level == 0 
                        && string.Equals(x.Area, area, StringComparison.CurrentCultureIgnoreCase));
    }
}