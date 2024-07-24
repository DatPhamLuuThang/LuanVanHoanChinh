using System.Reflection;
using System.Text.RegularExpressions;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using CoreEntities.RequestModel;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Identity;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.DataEntry.Controllers;

[Area("DataEntry")]
[Authorize(Policy = "Manager")]
[Route("-/[area]-[action]/[controller]")]
public partial class TeacherData : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "DataEntry";
    private const string CurrentTitle = "Quản lý Giáo viên";

    private static readonly PropertyInfo[] CurrentType = typeof(Teacher).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id", "IsActive", "ClassRooms", "Subjects" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Teaching", "Schedule" };


    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;


    private readonly IMapper _mapper;

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<Teacher> _teacherServices;

    #endregion

    #region Initialization

    public TeacherData(IMapper mapper, IMenuServices menuView, INotyfService notyfService,
        ICoreServices<Teacher> teacherServices,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        /* View Services */
        _mapper = mapper;
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
        _teacherServices = teacherServices;

        /* identity Services */
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index()
    {
        var data = await _teacherServices
            .GetAll()
            .OrderBy(x => x.Experience)
            .ThenBy(x => x.Specialize)
            .ThenBy(x => x.Qualification)
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
            var data = _teacherServices.GetById(id);

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

        var data = _teacherServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();

            Helper.NotyfAssist(await _teacherServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeacherRerquest data)
    {
        try
        {
            var userId = Guid.NewGuid();

            data.Id = Guid.NewGuid();

            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = data.Email,
                Email = data.Email,
                NormalizedEmail = data.Email?.ToUpper(),
                NormalizedUserName = data.Email?.ToUpper(),
                EmailConfirmed = false,
                PhoneNumber = data.PhoneNumber,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                Active = false
            };
            if (data.Email != null)
            {
                var result = await _userManager.CreateAsync(user, data.Email);

                if (result.Succeeded)
                {
                    ValidDataOnTeacher(data);
                    Helper.NotyfAssist(
                        await _teacherServices.AddAsync(_mapper.Map<CreateTeacherRerquest, Teacher>(data), userId),
                        _notyfService);

                    _notyfService.Success("Thêm giáo viên và tạo tài khoản thành công!");
                }
            }

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            _notyfService.Warning("dot");
            CreateOrUpdate();
            return View(Helper.StaticUrl.CreateOrUpdate, data);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Teacher data)
    {
        var userId = Guid.NewGuid();

        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _teacherServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _teacherServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _teacherServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _teacherServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _teacherServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(Teacher data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());

        data.Email = data.Email?.ToLower();
    }

    private static void ValidDataOnTeacher(CreateTeacherRerquest data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());

        data.Email = data.Email?.ToLower();
        data.PhoneNumber = data.PhoneNumber ?? "";

        data.PhoneNumber = MyRegex().Replace(data.PhoneNumber, "");
        if (data.PhoneNumber.Length != 10)
        {
            data.PhoneNumber = data.PhoneNumber.PadLeft(10, '0');
        }
    }

    [GeneratedRegex("[^\\d]")]
    private static partial Regex MyRegex();
    
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
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    #endregion
}