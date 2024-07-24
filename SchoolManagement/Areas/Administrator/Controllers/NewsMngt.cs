using System.Reflection;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.Administrator.Controllers;

[Authorize(Policy = "Admin")]
[Area("Administrator")]
[Route("-/[area]-[action]/[controller]")]
public class NewsMngt  : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "Administrator";
    private const string CurrentTitle = "Quản lí thông tin hỉnh ảnh tin tức";
    private static readonly PropertyInfo[] CurrentType = typeof(News).GetProperties();
    private static readonly List<string> NonAccessIndex = new List<string>
        { "Id","IsActive" };

    private static readonly List<string> NonAccessDetails = new List<string>
        { "Id" };


    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;
    
    private readonly IWebHostEnvironment _webHostEnvironment;

    /* Data Services */
    
    private readonly ICoreServices<News> _newsServices;

    #endregion

    #region Initialization

    public NewsMngt(IMenuServices menuView, INotyfService notyfService,
        ICoreServices<News> newsServices,IWebHostEnvironment webHostEnvironment)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;
        _webHostEnvironment = webHostEnvironment;

        /* Data Services */
        _newsServices = newsServices;
        
    }

    #endregion

    #region View Action

    public async Task<ActionResult> Index()
    {
        var data = await _newsServices
            .GetAll()
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        MainView();

        return View(data);
    }

    
    public IActionResult Create()
    {
        CreateOrUpdate();
        return View();
    }
    
    public IActionResult Edit(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var data = _newsServices.GetById(id);

            if (data != null)
            {
                CreateOrUpdate();
                return View(data);
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
        
        var data = _newsServices.GetById(id);

        if (data != null)
        {
            data.Id = Guid.NewGuid();
            
            Helper.NotyfAssist(await _newsServices.AddAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(News data)
    {
        var userId = Guid.NewGuid();

        if (data.ImgSchoolFile != null && data.ImgSchoolFile.Length > 0)
        {
            data.ImgSchoolFileName = await SaveImageFileAsync(data.ImgSchoolFile);
        }
        
        data.Id = Guid.NewGuid();

        ValidData(data);
        Helper.NotyfAssist(await _newsServices.AddAsync(data, userId), _notyfService);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, News data)
    {
        var userId = Guid.NewGuid();
        if (Guid.Parse(id) == data.Id)
        {
            if (data.ImgSchoolFile != null && data.ImgSchoolFile.Length > 0)
            {
                // Lưu tệp hình ảnh đã tải lên và cập nhật ImgSchoolFileName
                data.ImgSchoolFileName = await SaveImageFileAsync(data.ImgSchoolFile);
            }
            
            ValidData(data);
            Helper.NotyfAssist(await _newsServices.UpdateAsync(data, userId), _notyfService);
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

        var data = _newsServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _newsServices.SoftDeleteAsync(data, userId), _notyfService);
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

        var data = _newsServices.GetById(id);

        if (data != null)
        {
            Helper.NotyfAssist(await _newsServices.HardDeleteAsync(data, userId), _notyfService);
        }
        else
        {
            _notyfService.Warning(Helper.NotyfMsg.Warning);
        }

        return RedirectToAction(nameof(Index));
    }

    
    #endregion

    #region Other
    private static void ValidData(News data) //Valid data before CRUD, call before CRUD
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
    }
    
    private void SetupMenu() //Load menu on SideBar
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }
    
    private async Task<string> SaveImageFileAsync(IFormFile file)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        // Xây dựng đường dẫn đầy đủ đến thư mục "uploads"
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Kiểm tra xem thư mục "uploads" có tồn tại hay không, nếu không thì tạo mới
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }


    #endregion
}