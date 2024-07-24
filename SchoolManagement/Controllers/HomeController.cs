using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Models;
using SchoolManagement.Services.Interface;


namespace SchoolManagement.Controllers;

[Authorize]
[Route("-/[action]")]
public class HomeController : Controller
{
    #region Declare
    
    /* View Services */
    private readonly INotyfService _notyfService;
    
    /* Data Services */
    private readonly ICoreServices<SchoolDetail> _schoolDetailServices;
    private readonly ICoreServices<Teacher> _teacherServices;
    private readonly ICoreServices<News> _newsServices;
    private readonly ICoreServices<Student> _studentServices;

    
    #endregion
    
    
    public HomeController( INotyfService notyfService,
        ICoreServices<SchoolDetail> schoolDetailServices,
        ICoreServices<News> newsServicesServices,
        ICoreServices<Teacher> teacherServices,
        ICoreServices<Student> studentServices)
    {
        /* View Services */
        _notyfService = notyfService;
        
        /* Data Services */
        _schoolDetailServices = schoolDetailServices;
        _newsServices = newsServicesServices;
        _teacherServices = teacherServices;
        _studentServices = studentServices;
    }
    
    [Route("/")]
    public IActionResult Index()
    {
        return RedirectToAction("IndexLayout");
    }

    public async Task<IActionResult> IndexLayout()
    {
        var data = await _schoolDetailServices
            .GetAll()
            .OrderBy(x => x.Id)
            .ToListAsync();
        
        var news = await _newsServices
            .GetAll()
            .Take(5)
            .ToListAsync();

        var schoolImageFilePaths = _newsServices
            .GetAll()
            .Select(s => s.ImgSchoolFileName).ToList();
        
        var userEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == userEmail);

        var student = _studentServices
            .GetAll()
            .FirstOrDefault(x => x.Email == userEmail);
        
        var viewModel = new HomeIndexViewModel
        {
            SchoolDetails = data,
            News = news,
            SchoolImageFilePaths = schoolImageFilePaths!,
            Teacher = teacher!,
            Student = student!
        };

        _notyfService.Information("Chào mừng trở lại");
        return View(viewModel);
    }
}