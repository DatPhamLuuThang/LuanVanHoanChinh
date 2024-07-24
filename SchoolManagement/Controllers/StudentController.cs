using System.Security.Cryptography;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Models;
using SchoolManagement.Services.Interface;

namespace SchoolManagement.Controllers;

[Authorize(Policy = "Student")]
[Route("[Controller]/[action]")]
public class StudentController : Controller
{
    #region Declare

    private readonly INotyfService _notyfService;

    private readonly ICoreServices<Teacher> _teacherServices;
    private readonly ICoreServices<Student> _studentServices;
    private readonly ICoreServices<Transcript> _transcriptServices;
    private readonly ICoreServices<ClassRoom> _classroomServices;
    private readonly ICoreServices<Semester> _semesterServices;

    private readonly ICoreServices<Schedule> _scheduleServices;
    private readonly ICoreServices<Violation> _violationServices;

    #endregion
    
    #region Initialization

    public StudentController(INotyfService notyfService,
        ICoreServices<Teacher> teacherServices, ICoreServices<Student> studentServices,
        ICoreServices<Transcript> transcriptServices, ICoreServices<ClassRoom> classroomServices,
 ICoreServices<Schedule> scheduleServices,
        ICoreServices<Violation> violationServices,
        ICoreServices<Semester> semesterServices)
    {
        _notyfService = notyfService;

        _teacherServices = teacherServices;
        _transcriptServices = transcriptServices;
        _studentServices = studentServices;
        _classroomServices = classroomServices;

        _scheduleServices = scheduleServices;
        _violationServices = violationServices;
        _semesterServices = semesterServices;
    }

    #endregion
    
    //trang cá nhân học sinh
    public IActionResult Index()
    {
        _notyfService.Information("Trang cá nhân của bạn");

        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var student = _studentServices
            .GetAll()
            .Include(t=>t.ClassRoom)
            .FirstOrDefault(x => x.Email == studentEmail);

        return View(student);
    }
    
    //chỉnh sửa thông tin cá nhân học sinh
    public async Task<IActionResult> Edit()
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        
        if (string.IsNullOrEmpty(studentEmail))
        {
            _notyfService.Warning("Vui lòng đăng nhập lại");
            return RedirectToAction(nameof(Index));
        }
        
        var student = await _studentServices
            .GetElementAsync(x => !x.IsDeleted && x.Email == studentEmail);

        if (student == null)
        {
            return RedirectToAction("Index");
        }

        _notyfService.Information("Chỉnh sửa thông tin cá nhân");
        return View(student);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(Student student)
    {
        var thisUserId = HttpContext.Session.GetString("UserId");

        if (!ModelState.IsValid || string.IsNullOrEmpty(thisUserId))
        {
            if (string.IsNullOrEmpty(thisUserId))
            {
                _notyfService.Warning("Vui lòng đăng nhập lại");
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        Guid userId;

        try
        {
            userId = Guid.Parse(thisUserId);
        }
        catch
        {
            userId = Guid.Empty;
        }

        var result = await _studentServices.UpdateAsync(student, userId);

        if (result.IsSuccess)
        {
            _notyfService.Success("Cập nhật thông tin cá nhân thành công");
        }
        else
        {
            _notyfService.Error("Cập nhật thông tin cá nhân thất bại");
        }

        return RedirectToAction("Index");
    }
    
    //thời khóa biểu học sinh
    public IActionResult ViewSchedule()
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var student = _studentServices
            .GetAll()
            .Include(t => t.ClassRoom)
            .FirstOrDefault(x => x.Email == studentEmail);

        if (student == null || student.ClassRoom == null)
        {
            _notyfService.Warning("Học sinh không thuộc lớp nào hoặc thông tin lớp học không tồn tại");
            return RedirectToAction(nameof(Index));
        }

        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var schedules = _scheduleServices
            .GetAll()
            .Include(s => s.Subject)
            .Include(s => s.Subject!.Teacher)
            .Include(s => s.ScheduleTime)
            .Include(s => s.Semester)
            .Where(s => s.ClassRoomId == student.ClassRoomId &&
                        s.Day.Month == currentMonth &&
                        s.Day.Year == currentYear)
            .OrderBy(s => s.Day)
            .ToList();

        return View(schedules);
    }
    
    public IActionResult ListViolations()
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var student = _studentServices
            .GetAll()
            .Include(t => t.Violations)
            .FirstOrDefault(x => x.Email == studentEmail);

        if (student == null)
        {
            _notyfService.Warning("Học sinh không tồn tại hoặc không có quyền truy cập.");
            return RedirectToAction("Index");
        }
        var currentMonth = DateTime.Now.Month;
        
        var violations = student.Violations!
            .Where(v => v.DayOfViolate.HasValue && v.DayOfViolate.Value.Month == currentMonth)
            .ToList();

        return View(violations);
    }
    
    public List<Transcript> GetMonthlyTranscripts(Guid studentId, Guid semesterId, int month)
    {
        return _transcriptServices
            .GetAll()
            .Include(t => t.Subject)
            .Where(t => t.StudentId == studentId &&
                        t.SemesterId == semesterId &&
                        t.Month == month)
            .ToList();
    }
    
    private List<int> GetMonthsInSemester(Semester? semester)
    {
        if (semester == null || !semester.StartIn.HasValue || !semester.EndIn.HasValue)
        {
            return new List<int>();
        }

        List<int> monthsInSemester = new List<int>();

        DateTime currentDate = semester.StartIn.Value;

        while (currentDate <= semester.EndIn.Value)
        {
            monthsInSemester.Add(currentDate.Month);
            currentDate = currentDate.AddMonths(1);
        }

        return monthsInSemester;
    }
    
    public IActionResult ViewMonthsInSemester()
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var student = _studentServices
            .GetAll()
            .FirstOrDefault(x => x.Email == studentEmail);

        if (student == null)
        {
            _notyfService.Warning("Học sinh không tồn tại hoặc không có quyền truy cập.");
            return RedirectToAction("Index");
        }
        DateTime currentDate = DateTime.Now;
        
        var currentSemester = _semesterServices
            .GetAll()
            .OrderByDescending(s => s.StartIn)
            .FirstOrDefault(s => s.StartIn <= currentDate && s.EndIn >= currentDate)!;

        if (currentSemester == null)
        {
            _notyfService.Warning("Hiện không có học kỳ nào diễn ra.");
            return RedirectToAction("Index");
        }

        var monthsInSemester = GetMonthsInSemester(currentSemester);

        return View(monthsInSemester);
    }
    
    public IActionResult ViewTranscriptsByMonth(int month)
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var student = _studentServices
            .GetAll()
            .Include(t => t.ClassRoom)
            .FirstOrDefault(x => x.Email == studentEmail);

        if (student == null || student.ClassRoom == null)
        {
            _notyfService.Warning("Học sinh không thuộc lớp nào hoặc thông tin lớp học không tồn tại");
            return RedirectToAction(nameof(Index));
        }

        DateTime currentDate = DateTime.Now;
        
        var currentSemester = _semesterServices
            .GetAll()
            .OrderByDescending(s => s.StartIn)
            .FirstOrDefault(s => s.StartIn <= currentDate && s.EndIn >= currentDate)!;

        if (currentSemester == null)
        {
            _notyfService.Warning("Hiện không có học kỳ nào diễn ra.");
            return RedirectToAction("Index");
        }

        var monthlyTranscripts = GetMonthlyTranscripts(student.Id, currentSemester.Id, month);

        return View("ViewTranscriptsByMonth", monthlyTranscripts);
    }

    public IActionResult ViewSemesterTranscript()
    {
        var studentEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        
        var student = _studentServices
            .GetAll()
            .Include(t => t.ClassRoom)
            .Include(s => s.Transcripts)!
            .ThenInclude(t => t.Subject)
            .FirstOrDefault(x => x.Email == studentEmail);

        var violations = _violationServices
            .GetAll()
            .Where(v => v.StudentId == student!.Id && v.DayOfViolate.HasValue)
            .ToList();
        
        if (student == null || student.ClassRoom == null)
        {
            _notyfService.Warning("Học sinh không thuộc lớp nào hoặc thông tin lớp học không tồn tại");
            return RedirectToAction(nameof(Index));
        }
        
        var transcripts = _transcriptServices
            .GetAll()
            .Include(t => t.Subject)
            .Include(t => t.Semester)
            .Where(t => t.StudentId == student.Id)
            .ToList();
        
        int numberOfViolations = violations.Count;

        student!.KindofConduct = numberOfViolations switch
        {
            >= 5 => "Trung bình",
            >= 3 => "Khá",
            _ => "Tốt"
        };

        var totalScore = 0m;
        var totalSubjectWeight = 0m;

        foreach (var transcript in transcripts)
        {
            if (!transcript.IsDeleted)
            {
                var subjectWeight = GetSubjectWeight(transcript.TypeId);
                totalScore += transcript.Value * subjectWeight;
                totalSubjectWeight += subjectWeight;
            }
        }
        
        var averageScore = totalSubjectWeight > 0 ? totalScore / totalSubjectWeight : 0;

        if (averageScore >= (decimal)8.5)
        {
            student.Rank = "Giỏi";
        }
        else if (averageScore >= (decimal)6.5)
        {
            student.Rank = "Khá";
        }
        else
        {
            student.Rank = "Trung bình";
        }

        return View(student);

    }
    
    
    private decimal GetSubjectWeight(int typeId)
    {
        // Định nghĩa hệ số nhân cho từng loại điểm 
        switch (typeId)
        {
            case 1:
                return 1m;
            case 2:
                return 1m;
            case 3:
                return 2m;
            case 4:
                return 3m;

            default:
                return 1m;
        }
    }
        
}

