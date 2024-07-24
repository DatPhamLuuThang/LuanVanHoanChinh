using System.Reflection;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.DataEntry.Controllers;

[Area("DataEntry")]
[Authorize(Policy = "Manager")]
[Route("-/[area]-[action]/[controller]")]
public class TranscriptData : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "DataEntry";
    private const string CurrentTitle = "Quản lý bảng điểm";

    private static readonly PropertyInfo[] CurrentType = typeof(Transcript).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","Student","Subject", "Semester", "IsActive"};

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Student", "Subject", "Semester" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<Transcript> _transcriptServices;
    private readonly ICoreServices<Student> _studentServices;
    private readonly ICoreServices<Subject> _subjectServices;
    private readonly ICoreServices<Semester> _semesterServices;

    #endregion

    #region Initialization

    public TranscriptData(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<Transcript> transcriptServices,
        ICoreServices<Student> studentServices,
        ICoreServices<Subject> subjectServices,
        ICoreServices<Semester> semesterServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _transcriptServices = transcriptServices;
        _studentServices = studentServices;
        _subjectServices = subjectServices;
        _semesterServices = semesterServices;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index()
    {
        var data = await _transcriptServices
            .GetAll()
            .Include(x => x.Student)
            .Include(x => x.Subject)
            .Include(x => x.Semester)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        MainView();
        /* MainView() of IndexWithCustom must modify to use */

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
            var data = _transcriptServices.GetById(id);

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
        
        var data = _transcriptServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _transcriptServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Transcript data)
    {
        var userId = Guid.NewGuid();

        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _transcriptServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Transcript data)
    {
        var userId = Guid.NewGuid();

        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _transcriptServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _transcriptServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _transcriptServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _transcriptServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _transcriptServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(Transcript data) //Valid data before CRUD, call before CRUD
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
                Name= $"{x.FirstName} {x.LastName}"
            }), "Id", "Name");
        
        ViewBag.SubjectId = new SelectList(_subjectServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.NameSubject} ({x.Teacher.FirstName} {x.Teacher.LastName})"
            }), "Id", "Name");
        ViewBag.SemesterId = new SelectList(_semesterServices
            .GetAll()
            .Select(x => new
            {
                x.Id,
                Name = $"{x.NameSemester}"
            }), "Id", "Name");
        
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    #endregion
}