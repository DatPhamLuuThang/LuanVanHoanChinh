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
public class ScheduleMngt : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Quản lý lịch dạy";

    private static readonly PropertyInfo[] CurrentType = typeof(Schedule).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","IsActive","Semester","Subject","ClassRoom"};

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Teacher", "Subject", "ClassRoom", "Student" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<Schedule> _scheduleServices;
    private readonly ICoreServices<Semester> _semesterServices;
    private readonly ICoreServices<Subject> _subjectServices;
    private readonly ICoreServices<ClassRoom> _classroomServices;
    private readonly ICoreServices<ScheduleTime> _scheduletimeServices;

    #endregion


    #region Initialization

    public ScheduleMngt(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<Schedule> scheduleServices,
        ICoreServices<Semester> semesterServices,
        ICoreServices<Subject> subjectServices,
        ICoreServices<ClassRoom> classroomServices,
        ICoreServices<ScheduleTime> scheduletimeServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _scheduleServices = scheduleServices;
        _semesterServices = semesterServices;
        _subjectServices = subjectServices;
        _classroomServices = classroomServices;
        _scheduletimeServices = scheduletimeServices;
    }

    #endregion


    #region View Action

    public async Task<IActionResult> Index()
    {
        var data = await _scheduleServices
            .GetAll()
            .Include(x => x.Semester)
            .Include(x => x.Subject)
            .Include(x => x.ClassRoom)
            .Include(x => x.ScheduleTime)
            .OrderBy(x=>x.Day)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
        
        MainView();
        
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
            var data = _scheduleServices.GetById(id);

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
        
        var data = _scheduleServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _scheduleServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Schedule data)
    {
  
        var userId = Guid.NewGuid();
        
        // Kiểm tra xem đã có tiết học này trong ngày đó tại lớp đó học kỳ đó chưa
        var existingSchedule = _scheduleServices
            .GetAll()
            .FirstOrDefault(x => x.Day == data.Day
                                 && x.ClassRoomId == data.ClassRoomId
                                 && x.SemesterId == data.SemesterId
                                 && x.ScheduleTimeId == data.ScheduleTimeId);

        if (existingSchedule != null)
        {
            _notyfService.Error("Tiết học đã tồn tại trong thời khóa biểu");
            CreateOrUpdate();
            return View(Helper.StaticUrl.CreateOrUpdate, data);
        }
        
        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _scheduleServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Schedule data)
    {
        var userId = Guid.NewGuid();
        
        

        if (Guid.Parse(id) == data.Id)
        {
            // Kiểm tra xem đã có tiết học này trong ngày đó tại lớp đó học kỳ đó chưa
            var existingSchedule = _scheduleServices
                .GetAll()
                .FirstOrDefault(x => x.Day == data.Day
                                     && x.ClassRoomId == data.ClassRoomId
                                     && x.SemesterId == data.SemesterId
                                     && x.ScheduleTimeId == data.ScheduleTimeId);

            if (existingSchedule != null)
            {
                _notyfService.Error("Tiết học đã tồn tại trong thời khóa biểu");
                CreateOrUpdate();
                return View(Helper.StaticUrl.CreateOrUpdate, data);
            }
            ValidData(data);
            Helper.NotyfAssist(await _scheduleServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _scheduleServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _scheduleServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _scheduleServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _scheduleServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(Schedule data) //Valid data before CRUD, call before CRUD
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
        
        ViewBag.SemesterId = new SelectList(_semesterServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.NameSemester}"
            }), "Id","Name");
        
        ViewBag.SubjectId = new SelectList(_subjectServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.NameSubject} - {x.Teacher!.FirstName} {x.Teacher.LastName}"
            }), "Id", "Name");
        ViewBag.ClassRoomId = new SelectList(_classroomServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.ClassName} ({x.SchoolYear})"
            }), "Id", "Name");
        ViewBag.ScheduleTimeId = new SelectList(_scheduletimeServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.LessonName}"
            }), "Id", "Name");
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    #endregion
}