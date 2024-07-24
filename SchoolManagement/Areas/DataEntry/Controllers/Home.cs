using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.DataEntry.Controllers;

[Area("DataEntry")]
[Authorize(Policy = "Manager")]
[Route("-/[area]-[action]/[controller]")]
public class Home : Controller
{
    #region Declare

    /* View Data */
    private const string CurrentArea = "DataEntry";
    private const string CurrentTitle = "DataEntry Area";

    /* View Services */
    private readonly IMenuServices _menuView;
    private readonly INotyfService _notyfService;

    /* Data Services */

    #endregion

    #region Initialization

    public Home(IMenuServices menuView, INotyfService notyfService)
    {
        /* View Services */
        _menuView = menuView;
        _notyfService = notyfService;

        /* Data Services */
    }

    #endregion

    #region View Action

    public ActionResult Index()
    {
        _notyfService.Information("DataEntry Area!!!");
        MainView();
        return View(Helper.StaticUrl.Empty);
    }

    #endregion

    #region Process Action

    #endregion

    #region Other

    private void MainView()
    {
        SetupMenu();

        ViewData["Title"] = CurrentTitle;
    }

    private void SetupMenu()
    {
        ViewBag.Menu = _menuView.LoadMenu(CurrentArea);
    }

    #endregion
}