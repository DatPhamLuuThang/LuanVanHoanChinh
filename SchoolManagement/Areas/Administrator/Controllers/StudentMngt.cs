using System.Reflection;
using System.Text.RegularExpressions;
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
public partial class StudentMngt : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Quản lý Học sinh";
    private static readonly PropertyInfo[] CurrentType = typeof(Student).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","Parents","Transcripts","Violations", "IsActive", "ClassRoom" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Transcript", "Schedule" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<Student> _studentServices;
    private readonly ICoreServices<ClassRoom> _classroomServices;

    private readonly ICoreServices<Parents> _parentServices;

    #endregion

    #region Initialization

    public StudentMngt(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<Student> studentServices,
        ICoreServices<ClassRoom> classroomServices,
  
        ICoreServices<Parents> parentServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _studentServices = studentServices;
        _classroomServices = classroomServices;

        _parentServices = parentServices;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index(string? clasroomId)
    {
        var data = string.IsNullOrEmpty(clasroomId)
            ? await _studentServices
                .GetAll()
                .Include(m => m.ClassRoom)

                .Include(m => m.Parents)
                .OrderBy(x => x.Id)
                .ThenBy(x => x.ClassRoomId)
                .ToListAsync()
            : await _studentServices
                .GetAll()
                .Where(x => x.ClassRoomId == Guid.Parse(clasroomId))
                .Include(m => m.ClassRoom)

                .Include(m => m.Parents)
                .OrderBy(x => x.Id)
                .ThenBy(x => x.ClassRoomId)
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
            var data = _studentServices.GetById(id);

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
        
        var data = _studentServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _studentServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Student data)
    {
        var userId = Guid.NewGuid();

        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _studentServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Student data)
    {
        var userId = Guid.NewGuid();
        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _studentServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _studentServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _studentServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _studentServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _studentServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(Student data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());
        
        data.PhoneNumber = MyRegex().Replace(data.PhoneNumber!, "");
        if (data.PhoneNumber.Length != 10)
        {
            data.PhoneNumber = data.PhoneNumber.PadLeft(10, '0');
        }
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

        ViewBag.ClassRoomId = new SelectList(_classroomServices.GetAll(), "Id", "ClassName");
    }

    private void CreateOrUpdate() //Call before View() of CreateOrUpdate
    {
        SetupMenu();

        ViewBag.ClassRoomId = new SelectList(_classroomServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.ClassName} (Khóa: {x.SchoolYear})"
            }), "Id", "Name");
        
        ViewBag.ParentId = new SelectList(_parentServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.FirstName} ( {x.LastName} )"
            }), "Id", "Name");
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    [GeneratedRegex("[^\\d]")]
    private static Regex MyRegex()
    {
        throw new NotImplementedException();
    }

    #endregion
}