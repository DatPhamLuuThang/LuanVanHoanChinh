using System.Reflection;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.Administrator.Controllers;
[Authorize(Policy = "Manager")]
[Area("Administrator")]
[Route("-/[area]-[action]/[controller]")]
public class ViolationMngt : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Quản lý Vi phạm";
    private static readonly PropertyInfo[] CurrentType = typeof(Violation).GetProperties();
    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","IsActive","Student" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Id" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<Student> _studentServices;
    private readonly ICoreServices<Violation> _violateServices;


    #endregion

    #region Initialization

    public ViolationMngt(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<Student> studentServices,
        ICoreServices<Violation> violateServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _studentServices = studentServices;
        _violateServices = violateServices;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index(string? clasroomId)
    {
        var data = await _violateServices
            .GetAll()
            .Include(x=>x.Student)
            .OrderBy(x => x.DayOfViolate)
            .ToListAsync();
        
        MainView();
        
        return View( data);
    }

    public IActionResult Create()
    {
        CreateOrUpdate();
        return View(Helper.StaticUrl.CreateOrUpdate);
    }

    public IActionResult Edit(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var data = _violateServices.GetById(id);

            if (data != null)
            {
                CreateOrUpdate();
                return View(Helper.StaticUrl.CreateOrUpdate, data);
            }
        }

        _notyfService.Warning(Helper.NotyfMsg.Warning);
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Process Action

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(string id)
    {
        var userId = Guid.NewGuid();

        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));
        
        var data = _violateServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _violateServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Violation data)
    {
        var userId = Guid.NewGuid();

        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _violateServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Violation data)
    {
        var userId = Guid.NewGuid();
        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _violateServices.UpdateAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SoftDelete(string id)
    {
        var userId = Guid.NewGuid();

        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        var data = _violateServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _violateServices.SoftDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDelete(string id)
    {
        var userId = Guid.NewGuid();

        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        var data = _violateServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _violateServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(Violation data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());
    }

    private void MainView() //Call before View() of Index
    {
        SetupMenu();
        ViewData["Title"] = CurrentTitle;

        ViewBag.CurrentType = CurrentType;
        /* Only use in <"Index"> - Start */
        /* In <"IndexWithCustom"> must be edited before use  */

        ViewBag.NonAccessAtributeInIndex = NonAccessIndex;
        ViewBag.NonAccessAtributeInIndex.AddRange(Helper.DeniedAttribute.List);

        ViewBag.NonAccessAtributeInDetails = NonAccessDetails;

        /* Only use in <"Index"> - End */
        
    }

    private void CreateOrUpdate() //Call before View() of CreateOrUpdate
    {
        SetupMenu();

        ViewBag.StudentId = new SelectList(_studentServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.LastName} {x.FirstName}"
            }), "Id", "Name");
        
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }
    #endregion
}