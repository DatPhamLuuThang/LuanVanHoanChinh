using System.Reflection;
using System.Text.RegularExpressions;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.Administrator.Controllers;

[Authorize(Policy = "Manager")]
[Area("Administrator")]
[Route("-/[area]-[action]/[controller]")]
public partial class SchoolDetailMngt : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Thông tin trường học";
    private static readonly PropertyInfo[] CurrentType = typeof(SchoolDetail).GetProperties();

    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id", "ClassRooms","IsActive" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Id" };

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */
    private readonly ICoreServices<SchoolDetail> _schoolDetailServices;

    #endregion

    #region Initialization

    public SchoolDetailMngt(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<SchoolDetail> schoolDetailServices)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;
        /* Data Services */
        _schoolDetailServices = schoolDetailServices;
    }

    #endregion

    #region View Action

    public async Task<IActionResult> Index()
    {
        var data = await _schoolDetailServices
            .GetAll()
            .OrderBy(x => x.Id)
            .ToListAsync();

        MainView();
        /* MainView() of IndexWithCustom must modify to use */

        return View(Helper.StaticUrl.Index, data);
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
            var data = _schoolDetailServices.GetById(id);

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
    public async Task<IActionResult> Create(SchoolDetail data)
    {
        var userId = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _schoolDetailServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, SchoolDetail data)
    {
        var userId = Guid.NewGuid();

        if (Guid.Parse(id) == data.Id)
        {
            ValidData(data);
            Helper.NotyfAssist(await _schoolDetailServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _schoolDetailServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _schoolDetailServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _schoolDetailServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _schoolDetailServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(string id)
    {
        var userId = Guid.NewGuid();

        if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));

        var data = _schoolDetailServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _schoolDetailServices.DuplicateAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Other

    private static void ValidData(SchoolDetail data) //Valid data before CRUD, call before CRUD
    {
        foreach (var property in data.GetType().GetProperties())
            if (property.GetValue(data) is string)
                property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());

        data.Email = data.Email.ToUpper();
        data.Phone = MyRegex().Replace(data.Phone, "");
        if (data.Phone.Length != 10)
        {
            data.Phone = data.Phone.PadLeft(10, '0');
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
    }

    private void CreateOrUpdate() //Call before View() of CreateOrUpdate
    {
        SetupMenu();
    }

    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    [GeneratedRegex("[^\\d]")]
    private static partial Regex MyRegex();

    #endregion
}