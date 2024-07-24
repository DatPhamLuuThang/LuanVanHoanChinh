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
public class ClassRoomMngt : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Quản lý Lớp học";

    private static readonly PropertyInfo[] CurrentType = typeof(ClassRoom).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","SchoolDetail", "IsActive", "SchoolDetailId","Teacher", "Schedules", "Students" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Id","SchoolDetail","SchoolDetailId","Schedules","Students" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<ClassRoom> _classroomServices;
    private readonly ICoreServices<Teacher> _teacherServices;
    private readonly ICoreServices<SchoolDetail> _schooldetailServices;

    #endregion

    #region Initialization

    public ClassRoomMngt(IMenuServices menuView,
        INotyfService notyfService,
        ICoreServices<ClassRoom> clasroomServices,
        ICoreServices<Teacher> teacherServices,
        ICoreServices<SchoolDetail> schooldetailServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _classroomServices = clasroomServices;
        _teacherServices = teacherServices;
        _schooldetailServices = schooldetailServices;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index()
    {
        var data = await _classroomServices
            .GetAll()
            .Include(x=>x.Teacher)
            .Include(x=>x.SchoolDetail)
            .OrderBy(x => x.ClassName)
            .ThenBy(x => x.SchoolYear)
            .ThenBy(x => x.Total)
            .ToListAsync();

        MainView();
        /* MainView() of IndexWithCustom must modify to use */

        return View(data);
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
            var data = _classroomServices.GetById(id);

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
        
        var data = _classroomServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _classroomServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassRoom data)
    {
        var userId = Guid.NewGuid();

        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _classroomServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ClassRoom data)
    {
        var userId = Guid.NewGuid();

        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _classroomServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _classroomServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _classroomServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _classroomServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _classroomServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(ClassRoom data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());
    }

    private void MainView() //Call before View() of Index
    {
        SetupMenu();
        ViewData["Title"] = CurrentTitle;

        /* Only use in <"Index"> - Start */
        /* In <"IndexWithCustom"> must be edited before use  */

        ViewBag.CurrentType = CurrentType;

        ViewBag.NonAccessAtributeInIndex = NonAccessIndex;
        ViewBag.NonAccessAtributeInIndex.AddRange(Helper.DeniedAttribute.List);

        ViewBag.NonAccessAtributeInDetails = NonAccessDetails;

        /* Only use in <"Index"> - End */
    }

    private void CreateOrUpdate() //Call before View() of CreateOrUpdate
    {
        SetupMenu();
        ViewBag.TeacherId = new SelectList(_teacherServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.FirstName} ({x.LastName})"
            }), "Id", "Name");
        ViewBag.DetailId = new SelectList(_schooldetailServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.SchoolName}"
            }), "Id", "Name");
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    #endregion
}