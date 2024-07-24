using System.Reflection;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.Administrator.Controllers
{
    [Authorize(Policy = "Manager")]
    [Area("Administrator")]
    [Route("-/[area]-[action]/[controller]")]
    public class MenuManagement : Controller
    {
        #region Declare

        /* View Data */
        private const string CurrentArea = "Administrator";
        private const string CurrentTitle = "Quản lý Menu";

        private static readonly PropertyInfo[] CurrentType = typeof(Menu).GetProperties();

        private static readonly List<string> NonAccessIndex = new List<string>
            { "Id", "IsActive", "ParentId", "Parent", "Child", "AttributeTable" };

        private static readonly List<string> NonAccessDetails = new List<string>
            { "Parent", "Child", "AttributeTable" };

        /* View Services */
        private readonly IMenuServices _menuView;
        private readonly INotyfService _notyfService;

        /* Data Services */
        private readonly ICoreServices<Menu> _menuServices;

        #endregion

        #region Initialization

        public MenuManagement(IMenuServices menuView, 
            INotyfService notyfService,
            ICoreServices<Menu> menuServices)
        {
            /* View Services */
            _menuView = menuView;
            _notyfService = notyfService;

            /* Data Services */
            _menuServices = menuServices;
        }

        #endregion

        #region View Action

        public async Task<IActionResult> Index()
        {
            var data = await _menuServices
                .GetAll()
                .Include(m => m.Parent)
                .OrderBy(x=>x.Level)
                .ThenBy(x=>x.Order)
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
                var data = _menuServices.GetById(id);

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
        public async Task<IActionResult> Create(Menu data)
        {
            var userId = Guid.NewGuid();

            data.Id = Guid.NewGuid();

            ValidData(data);
            Helper.NotyfAssist(await _menuServices.AddAsync(data, userId), _notyfService);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Menu data)
        {
            var userId = Guid.NewGuid();

            if (Guid.Parse(id) == data.Id)
            {
                ValidData(data);
                Helper.NotyfAssist(await _menuServices.UpdateAsync(data, userId), _notyfService);
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

            var data = _menuServices.GetById(id);

            if (data != null)
            {
                Helper.NotyfAssist(await _menuServices.SoftDeleteAsync(data, userId), _notyfService);
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

            var data = _menuServices.GetById(id);

            if (data != null)
            {
                Helper.NotyfAssist(await _menuServices.HardDeleteAsync(data, userId), _notyfService);
            }
            else
            {
                _notyfService.Warning(Helper.NotyfMsg.Warning);
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Other

        private static void ValidData(Menu data) //Valid data before CRUD, call before CRUD
        {
            foreach (var property in data.GetType().GetProperties())
                if (property.GetValue(data) is string)
                    property.SetValue(data, property.GetValue(data)?.ToString()?.Trim());
            
            data.ParentId = data.Level == 0 ? data.Id : data.ParentId;
            data.Name = data.Level == 0 ? data.Name.ToUpper() : data.Name;
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
            //ViewBag.NonAccessAtributeInDetails.AddRange(Helper.DeniedAttribute.List);
            
            /* Only use in <"Index"> - End */
        }

        private void CreateOrUpdate() //Call before View() of CreateOrUpdate
        {
            SetupMenu();

            ViewBag.ParentId = new SelectList(_menuServices.GetAll().Where(x => x.Level == 0), "Id", "Name");
        }

        private void SetupMenu() //Load menu on SideBar
        {
            ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
        }

        #endregion
    }
}