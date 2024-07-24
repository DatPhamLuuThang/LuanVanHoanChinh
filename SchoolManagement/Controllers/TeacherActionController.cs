using System.Diagnostics.CodeAnalysis;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Identity;
using SchoolManagement.Models;
using SchoolManagement.Services.Interface;

namespace SchoolManagement.Controllers;

[Authorize(Policy = "Teacher")]
[Route("[Controller]/[action]")]
public class TeacherActionController : Controller
{
    #region Declare

    private readonly INotyfService _notyfService;

    private readonly ICoreServices<Teacher> _teacherServices;
    private readonly ICoreServices<Student> _studentServices;
    private readonly ICoreServices<Transcript> _transcriptServices;
    private readonly ICoreServices<ClassRoom> _classroomServices;
    private readonly ICoreServices<Violation> _violationServices;
    private readonly ICoreServices<Schedule> _scheduleServices;
    private readonly ICoreServices<Subject> _subjectServices;
    private readonly ICoreServices<Semester> _semesterServices;

    #endregion

    #region Initialization

    public TeacherActionController(INotyfService notyfService,
        ICoreServices<Teacher> teacherServices, ICoreServices<Student> studentServices,
        ICoreServices<Transcript> transcriptServices, ICoreServices<ClassRoom> classroomServices,
        ICoreServices<Schedule> scheduleServices, ICoreServices<Violation> violationServices,
        ICoreServices<Subject> subjectServices, ICoreServices<Semester> semesterServices)
    {
        _notyfService = notyfService;

        _teacherServices = teacherServices;
        _transcriptServices = transcriptServices;
        _studentServices = studentServices;
        _classroomServices = classroomServices;

        _scheduleServices = scheduleServices;
        _subjectServices = subjectServices;
        _violationServices = violationServices;
        _semesterServices = semesterServices;
    }

    #endregion

    //Trang cá nhân
    public IActionResult IndexTeacher()
    {
        _notyfService.Information("Trang cá nhân của bạn");

        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        return View(teacher);
    }

    //Chỉnh sửa thông tin cá nhân của giáo viên
    public async Task<IActionResult> EditInfoTeacher()
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        if (string.IsNullOrEmpty(teacherEmail))
        {
            _notyfService.Warning("Vui lòng đăng nhập lại");
            return RedirectToAction(nameof(IndexTeacher));
        }

        var teacher = await _teacherServices
            .GetElementAsync(x => !x.IsDeleted && x.Email == teacherEmail);

        if (teacher == null)
        {
            return RedirectToAction("IndexTeacher");
        }

        _notyfService.Information("Chỉnh sửa thông tin cá nhân");
        return View(teacher);
    }

    [HttpPost]
    public async Task<IActionResult> EditInfoTeacher(Teacher teacher)
    {
        var thisUserId = HttpContext.Session.GetString("UserId");

        if (!ModelState.IsValid || string.IsNullOrEmpty(thisUserId))
        {
            if (string.IsNullOrEmpty(thisUserId))
            {
                _notyfService.Warning("Vui lòng đăng nhập lại");
                return RedirectToAction(nameof(IndexTeacher));
            }

            return View(teacher);
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

        var result = await _teacherServices.UpdateAsync(teacher, userId);

        if (result.IsSuccess)
        {
            _notyfService.Success("Cập nhật thông tin cá nhân thành công");
        }
        else
        {
            _notyfService.Error("Cập nhật thông tin cá nhân thất bại");
        }

        return RedirectToAction("IndexLayout", "Home");
    }

    //Danh sách học sinh lớp chủ nhiệm
    public IActionResult ListStudentInClass()
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        var tws = new TeacherWithStudentModels
        {
            Teacher = teacher,
            ClassRooms = new List<ClassRoom>(),
            Students = new List<Student>()
        };

        if (teacher != null)
        {
            tws.ClassRooms = _classroomServices
                .GetAll()
                .Where(x => x.TeacherId == teacher.Id)
                .ToList();

            if (tws.ClassRooms.Any())
            {
                var firstClassId = tws.ClassRooms.First().Id;
                tws.Students = _studentServices
                    .GetAll()
                    .Include(t => t.ClassRoom)
                    .Where(x => x.ClassRoomId == firstClassId)
                    .OrderBy(x => x.Id)
                    .ToList();
            }
            else
            {
                _notyfService.Warning("Giáo viên chưa được phân công chủ nhiệm lớp nào");
                return RedirectToAction("IndexTeacher");
            }
        }

        _notyfService.Information("Danh sách học sinh");
        return View(tws);
    }

    //Cập nhật thông tin cá nhân học sinh lớp chủ nhiệm
    public IActionResult EditInfoStudent(Guid studentId)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var isStudentInTeacherClass = _classroomServices
            .GetAll()
            .Any(c => c.Students != null
                      && c.TeacherId == teacher.Id
                      && c.Students.Any(s => s.Id == studentId));

        if (!isStudentInTeacherClass)
        {
            _notyfService.Error("Học sinh không thuộc lớp chủ nhiệm của bạn");
            return RedirectToAction("IndexTeacher");
        }

        var student = _studentServices
            .GetAll()
            .FirstOrDefault(s => s.Id == studentId);

        return View(student);
    }

    [HttpPost]
    public IActionResult EditInfoStudent(Student student)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var isStudentInTeacherClass = _classroomServices
            .GetAll()
            .Any(c => c.Students != null
                      && c.TeacherId == teacher.Id
                      && c.Students.Any(s => s.Id == student.Id));
        if (!isStudentInTeacherClass)
        {
            _notyfService.Error("Học sinh không thuộc lớp chủ nhiệm của bạn");
            return RedirectToAction("ListStudentInClass");
        }

        if (ModelState.IsValid)
        {
            _studentServices.UpdateAsync(student, teacher.Id);

            _notyfService.Success("Cập nhật thông tin học sinh thành công");
            return RedirectToAction("ListStudentInClass");
        }

        return View(student);
    }

    //Danh sách vi phạm của học sinh lớp chủ nhiệm
    public IActionResult ViolateOfStudent(Guid studentId)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }
        var currentMonth = DateTime.Now.Month;
        
        var student = _studentServices
            .GetAll()
            .Include(s => s.Violations)
            .FirstOrDefault(s => s.Id == studentId);
        return View(student);
    }

    //Hiển thị lịch giảng dạy của giáo viên
    public IActionResult ScheduleOfTeacher()
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var currentMonth = DateTime.Now.Month;

        // Lấy danh sách lịch giảng dạy của giáo viên
        var scheduleList = _scheduleServices
            .GetAll()
            .Include(s => s.ClassRoom)
            .Include(s => s.Subject)
            .Include(s => s.Semester)
            .Where(s => s.ClassRoom != null && s.Subject != null
                                            && s.Subject.TeacherId == teacher.Id
                                            && s.Day.Month == currentMonth)
            .Select(s => new ScheduleViewModel
            {
                Day = s.Day,
                StartTime = s.ScheduleTime!.StartTime,
                ClassRoomName = s.ClassRoom!.ClassName!,
                SubjectName = s.Subject!.NameSubject!,
                SemesterName = s.Semester!.NameSemester!,
                LessonName = s.ScheduleTime!.LessonName,
                EndTime = s.ScheduleTime.EndTime
            })
            .OrderBy(x => x.LessonName)
            .ThenBy(x => x.Day)
            .ToList();

        return View(scheduleList);
    }

    //kiểm tra ngày thuộc tuần thi 
    private bool IsInExamWeek(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue)
        {
            TimeSpan duration = end.Value - start.Value;
            int examWeeks = (int)Math.Ceiling((decimal)duration.TotalDays / 7);

            DateTime currentDate = DateTime.Now;
            return currentDate >= start.Value && currentDate <= end.Value.AddDays(examWeeks * 7);
        }
        return false;
    }

    //kiểm tra ngày trong tuần nhập điểm
    private bool IsInputExam(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue)
        {
            DateTime scoreInputStart = start.Value.AddDays(1);
            DateTime scoreInputEnd = scoreInputStart.AddDays(6);

            DateTime currentDate = DateTime.Now;
            return currentDate >= scoreInputStart && currentDate <= scoreInputEnd;
        }

        return false;
    }

    //Thêm điểm bộ môn cho học  các lớp học mình đang dạy
    public IActionResult AddScore(Guid studentId, Guid subjectId, Guid semesterId)
    {
        var student = _studentServices.GetById(studentId.ToString());
        var subject = _subjectServices.GetById(subjectId.ToString());
        var semester = _semesterServices.GetById(semesterId.ToString());

        if (student == null || subject == null || semester == null)
        {
            _notyfService.Warning("Không tìm thấy bảng điểm");
            return View("AddScore");
        }

        var isExamWeek = IsInExamWeek(semester.StartIn, semester.EndIn);
        var currentDate = DateTime.Now;


        var startInput = semester.StartIn!.Value.AddDays((semester.ExamWeek + 1) * 7);
        var endInput = startInput.AddDays(6);

        var inputTime = IsInputExam(startInput, endInput);

        var currentSemester = _semesterServices.GetById(semesterId.ToString());
        ViewBag.SemesterId = currentSemester!.Id;

        ViewBag.Month = currentDate;

        ViewBag.Student = student;
        ViewBag.Subject = subject;
        ViewBag.Semester = semester;
        ViewBag.IsExamWeek = isExamWeek;

        ViewBag.InputTime = inputTime;

        _notyfService.Information("Thêm điểm số cho học sinh");
        return View("AddScore");
    }

    [HttpPost]
    public async Task<IActionResult> AddScore(Transcript transcript)
    {
        var thisUserId = HttpContext.Session.GetString("UserId");

        var student = _studentServices.GetById(transcript.StudentId.ToString());
        var subject = _subjectServices.GetById(transcript.SubjectId.ToString());
        var semester = _semesterServices.GetById(transcript.SemesterId.ToString());

        if (student == null || subject == null || semester == null)
        {
            _notyfService.Warning("Không tìm thấy bảng điểm");
            return View("AddScore");
        }

        Guid userId;

        try
        {
            userId = Guid.Parse(thisUserId!);
        }
        catch
        {
            userId = Guid.Empty;
        }


        if (transcript.TypeId == 4)
        {
            transcript.Month = null;
        }
        else
        {
            var currentMonth = DateTime.Now.Month;
            transcript.Month = currentMonth;
        }

        if (Valid(transcript))
        {
            await _transcriptServices.AddAsync(transcript, userId);
            _notyfService.Success("Thêm điểm thành công");
        }
        else
        {
            _notyfService.Warning("Thêm điểm thất bại");
        }

        return RedirectToAction("ClassStudents", new { classRoomId = student.ClassRoomId , subjectId  = subject.Id});
    }

    //Chỉnh sủa điểm cho học sinh lớp đang dạy
    public IActionResult EditScore(Guid studentId, Guid subjectId, Guid semesterId, int typeId)
    {
        var student = _studentServices.GetById(studentId.ToString());
        var subject = _subjectServices.GetById(subjectId.ToString());
        var semester = _semesterServices.GetById(semesterId.ToString());

        if (student == null || subject == null || semester == null)
        {
            _notyfService.Warning("Không tìm thấy bảng điểm");
            return RedirectToAction("CurrentClasses");
        }

        var currentMonth = DateTime.Now.Month;
        var currentSemester = _semesterServices.GetById(semesterId.ToString());

        // Lấy danh sách điểm hiện tại của loại điểm được chọn (trả bài, kiểm tra 15 phút, kiểm tra 1 tiết, ...)
        var existingScores = _transcriptServices
            .GetAll()
            .Where(t => t.StudentId == studentId
                        && t.SubjectId == subjectId
                        && t.SemesterId == semesterId
                        && t.TypeId == typeId)
            .ToList();


        ViewBag.Month = currentMonth;
        ViewBag.SemesterId = currentSemester!.Id;

        ViewBag.Student = student;
        ViewBag.Subject = subject;
        ViewBag.Semester = semester;

        var x = existingScores.Select(x => new
        {
            Value = x.Id,
            Text = x.Value
        }).ToList();

        ViewData["ScoreSlect"] = new SelectList(x, "Value", "Text");
        _notyfService.Information("Chỉnh sửa điểm số cho học sinh");
        return View("EditScore", new TranscriptEditRequestModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditScore(TranscriptEditRequestModel model)
    {
        var thisUserId = HttpContext.Session.GetString("UserId");

        var transcript = _transcriptServices.GetById(model.TranId.ToString());
        var student = _studentServices.GetById(transcript.StudentId.ToString());

        if (transcript == null)
        {
            _notyfService.Warning("Không tìm thấy bảng điểm");
            return RedirectToAction("EditScore", model);
        }

        Guid userId;
        try
        {
            userId = Guid.Parse(thisUserId!);
        }
        catch
        {
            userId = Guid.Empty;
        }

        var currentMonth = DateTime.Now.Month;

        transcript.Month = currentMonth;
        transcript.Value = model.newValue;
        transcript.IsDeleted = model.newValue == 0;
        if (Valid(transcript))
        {
            await _transcriptServices.UpdateAsync(transcript, userId);
            _notyfService.Success("Chỉnh sửa điểm thành công");
        }
        else
        {
            _notyfService.Warning("Chỉnh sửa điểm thất bại");
        }

        return RedirectToAction("ClassStudents", new { classRoomId = student!.ClassRoomId , subjectId  = transcript.SubjectId});
    }

    // Hiển thị các lớp đang dạy
    public IActionResult CurrentClasses()
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var currentMonth = DateTime.Now.Month;

        var currentClasses = _scheduleServices
            .GetAll()
            .Include(s => s.Subject)
            .Include(s => s.ClassRoom)
            .Include(s => s.Semester)
            .Include(s => s.ScheduleTime)
            .Where(s => s.Subject != null && s.Subject.TeacherId == teacher.Id && s.Day.Month == currentMonth)
            .OrderBy(x => x.Day)
            .GroupBy(s => s.ClassRoomId)
            .Select(group => group.First())
            .ToList();

        return View("CurrentClasses", currentClasses);
    }


    public async Task<IActionResult> ReportClass(Guid classroomId)
    {
        var now = DateTime.Now;
        var semester = _semesterServices
            .GetAll()
            .Where(x => x.StartIn != null && x.EndIn != null)
            .FirstOrDefault(x => x.StartIn!.Value <= now && now <= x.EndIn!.Value);

        var listStudent = await _studentServices.GetAll()
            .Where(x => x.ClassRoomId == classroomId)
            .ToListAsync();

        var listSubject = await _scheduleServices.GetAll()
            .Include(x => x.Subject)
            .Where(x => x.SemesterId == semester!.Id && x.ClassRoomId == classroomId)
            .Select(x => x.Subject)
            .ToListAsync();

        var studentAverages = new List<StudentAverageDTO>();
        var currentMonth = DateTime.Now.Month;


        foreach (var student in listStudent)
        {
            var studentAverage = new StudentAverageDTO
            {
                StudentName = $"{student.FirstName} {student.LastName}",
                SubjectAverages = new List<Tuple<string, double?>>()
            };

            var processedSubjects = new List<string>();

            foreach (var subject in listSubject)
            {
                if (processedSubjects.Contains(subject.NameSubject!))
                    continue;

                var transcripts = await _transcriptServices.GetAll()
                    .Where(t => t.StudentId == student.Id
                                && t.SubjectId == subject!.Id
                                && t.SemesterId == semester!.Id
                                && t.Month == currentMonth)
                    .ToListAsync();

                decimal tongHs1 = 0;
                decimal tongHs2 = 0;
                int tongsl1 = 0;
                int tongsl2 = 0;

                foreach (var studentTr in transcripts.Where(x => x.Month == DateTime.Now.Month))
                {
                    if (studentTr.TypeId != 4)
                    {
                        if (studentTr.TypeId == 1 || studentTr.TypeId == 2)
                        {
                            tongHs1 += studentTr.Value;
                            tongsl1 += 1;
                        }
                        else
                        {
                            tongHs2 += studentTr.Value;
                            tongsl2 += 1;
                        }
                    }
                }

                decimal tongket = (tongHs1 + tongHs2 * 2) /
                                  ((tongsl1 + tongsl2 * 2) == 0 ? 1 : (tongsl1 + tongsl2 * 2));

                studentAverage.SubjectAverages.Add(new Tuple<string, double?>(subject!.NameSubject!, (double)tongket));
                processedSubjects.Add(subject.NameSubject!);
            }

            studentAverages.Add(studentAverage);
        }


        return View("ReportClass", studentAverages);
    }

    // Hiển thị danh sách học sinh 1 lớp cụ thể
    public IActionResult ClassStudents(Guid classRoomId, Guid subjectId)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var studentsInClass = GetStudentsInClass(classRoomId);

        var scheduleInfo = _scheduleServices
            .GetAll()
            .Include(s => s.Subject)
            .Include(s => s.ClassRoom)
            .Include(s => s.ScheduleTime)
            .Include(x => x.Semester)
            .FirstOrDefault(x => x.ClassRoomId == classRoomId && x.SubjectId == subjectId);


        var listStudentWithTranscripts = studentsInClass.Select(x =>
            new StudentWithTranscript
            {
                StudentId = x.Id,
                Students = x,
                Transcripts =
                    x.Transcripts?.Where(t => !t.IsDeleted && t.SubjectId == scheduleInfo!.SubjectId).ToList()!
            }
        ).ToList();

        var model = new ClassStudentsViewModel
        {
            Teacher = teacher,
            SubjectId = scheduleInfo!.SubjectId,
            SemesterId = scheduleInfo.SemesterId,
            ClassRoom = _classroomServices.GetById(classRoomId.ToString())!,
            Students = listStudentWithTranscripts
        };

        return View("ClassStudents", model);
    }

    // Action để hiển thị danh sách điểm của một học sinh cho tất cả các môn tháng hiejne tại
    public IActionResult StudentTranscripts(Guid studentId)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var isStudentInTeacherClass = _classroomServices
            .GetAll()
            .Any(c => c.Students != null
                      && c.TeacherId == teacher.Id
                      && c.Students.Any(s => s.Id == studentId));

        if (!isStudentInTeacherClass)
        {
            _notyfService.Error("Học sinh không thuộc lớp chủ nhiệm của bạn");
            return RedirectToAction("ListStudentInClass");
        }

        // Lấy thông tin học sinh
        var student = _studentServices
            .GetAll()
            .Include(s => s.Transcripts)!
            .ThenInclude(t => t.Subject)
            .FirstOrDefault(s => s.Id == studentId);

        if (student == null)
        {
            _notyfService.Error("Không tìm thấy thông tin học sinh");
            return RedirectToAction("ListStudentInClass");
        }

        // Tạo ViewModel để hiển thị danh sách điểm
        var studentTranscriptViewModel = new StudentTranscriptViewModel
        {
            Teacher = teacher,
            Student = student,
        };

        return View(studentTranscriptViewModel);
    }

    // cập nhật học lực hạnh kiểm cho cả lớp
    [HttpPost]
    public async Task<IActionResult> UpdateConductAndBehaviorForClass()
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        var tws = new TeacherWithStudentModels
        {
            Teacher = teacher,
            ClassRooms = new List<ClassRoom>(),
            Students = new List<Student>(),
        };

        if (teacher != null)
        {
            tws.ClassRooms = _classroomServices
                .GetAll()
                .Where(x => x.TeacherId == teacher.Id)
                .ToList();

            var firstClassId = tws.ClassRooms.First().Id;
            tws.Students = _studentServices
                .GetAll()
                .Include(t => t.ClassRoom)
                .Where(x => x.ClassRoomId == firstClassId)
                .OrderBy(x => x.Id)
                .ToList();
        }

        foreach (var student in tws.Students)
        {
            await UpdateStudentConductAndBehavior(student);
        }

        _notyfService.Success("Cập nhật học lực và hạnh kiểm cho tất cả học sinh thành công");

        return RedirectToAction("ListStudentInClass");
    }

    //cập nhật học lực hạnh kiểm của 1 học sinh
    public async Task UpdateStudentConductAndBehavior(Student student)
    {
        var thisUserId = HttpContext.Session.GetString("UserId");
        Guid userId;
        try
        {
            userId = Guid.Parse(thisUserId!);
        }
        catch
        {
            userId = Guid.Empty;
        }

        var currentMonth = DateTime.Now.Month;

        var violations = _violationServices
            .GetAll()
            .Where(v => v.StudentId == student.Id
                        && v.NumberOfViolate.HasValue
                        && v.DayOfViolate!.Value.Month == currentMonth)
            .ToList();

        int numberOfViolations = violations.Count;

        student.KindofConduct = numberOfViolations switch
        {
            >= 5 => "Trung bình",
            >= 3 => "Khá",
            _ => "Tốt"
        };

        var transcripts = _transcriptServices
            .GetAll()
            .Include(t => t.Subject)
            .Include(t => t.Semester)
            .Where(t => t.StudentId == student.Id && t.Month == currentMonth)
            .ToList();

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

        await _studentServices.UpdateAsync(student, userId);
    }

    //thống kê cuối kì
    public IActionResult ReportTranscripts(Guid studentId)
    {
        var teacherEmail = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == teacherEmail);

        if (teacher == null)
        {
            _notyfService.Error("Không tìm thấy thông tin giáo viên");
            return RedirectToAction("IndexTeacher");
        }

        var isStudentInTeacherClass = _classroomServices
            .GetAll()
            .Any(c => c.Students != null
                      && c.TeacherId == teacher.Id
                      && c.Students.Any(s => s.Id == studentId));

        if (!isStudentInTeacherClass)
        {
            _notyfService.Error("Học sinh không thuộc lớp chủ nhiệm của bạn");
            return RedirectToAction("ListStudentInClass");
        }

        // Lấy thông tin học sinh
        var student = _studentServices
            .GetAll()
            .Include(s => s.Transcripts)!
            .ThenInclude(t => t.Subject)
            .FirstOrDefault(s => s.Id == studentId);


        var violations = _violationServices
            .GetAll()
            .Where(v => v.StudentId == student!.Id && v.DayOfViolate.HasValue)
            .ToList();

        var transcripts = _transcriptServices
            .GetAll()
            .Include(t => t.Subject)
            .Include(t => t.Semester)
            .Where(t => t.StudentId == student!.Id)
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


        if (student == null)
        {
            _notyfService.Error("Không tìm thấy thông tin học sinh");
            return RedirectToAction("ListStudentInClass");
        }

        // Tạo ViewModel để hiển thị danh sách điểm
        var studentTranscriptViewModel = new StudentTranscriptViewModel
        {
            Teacher = teacher,
            Student = student,
        };

        return View(studentTranscriptViewModel);
    }

    //Hệ số nhân của từng loại điểm
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


    [SuppressMessage("ReSharper.DPA", "DPA0006: Large number of DB commands", MessageId = "count: 540")]
    public async Task<IActionResult> ReportSemester(Guid classroomId)
    {
        var now = DateTime.Now;
        var semester = _semesterServices
            .GetAll()
            .Where(x => x.StartIn != null && x.EndIn != null)
            .FirstOrDefault(x => x.StartIn!.Value <= now && now <= x.EndIn!.Value);

        var listStudent = await _studentServices.GetAll()
            .Where(x => x.ClassRoomId == classroomId)
            .ToListAsync();

        var listSubject = await _scheduleServices.GetAll()
            .Include(x => x.Subject)
            .Where(x => x.SemesterId == semester!.Id && x.ClassRoomId == classroomId)
            .Select(x => x.Subject)
            .ToListAsync();

        var studentAverages = new List<StudentAverageDTO>();

        foreach (var student in listStudent)
        {
            var studentAverage = new StudentAverageDTO
            {
                StudentName = $"{student.FirstName} {student.LastName}",
                SubjectAverages = new List<Tuple<string, double?>>()
            };

            var processedSubjects = new List<string>();

            foreach (var subject in listSubject)
            {
                if (processedSubjects.Contains(subject!.NameSubject!))
                    continue;

                var transcripts = await _transcriptServices.GetAll()
                    .Where(t => t.StudentId == student.Id
                                && t.SubjectId == subject!.Id
                                && t.SemesterId == semester!.Id)
                    .ToListAsync();

                decimal tongHs1 = 0;
                decimal tongHs2 = 0;
                decimal tongHs3 = 0;
                int tongsl1 = 0;
                int tongsl2 = 0;
                int tongsl3 = 0;

                foreach (var studentTr in transcripts)
                {
                    if (studentTr.TypeId == 1 || studentTr.TypeId == 2)
                    {
                        tongHs1 += studentTr.Value;
                        tongsl1 += 1;
                    }
                    else if (studentTr.TypeId == 3)
                    {
                        tongHs2 += studentTr.Value;
                        tongsl2 += 1;
                    }
                    else if (studentTr.TypeId == 4)
                    {
                        tongHs3 += studentTr.Value;
                        tongsl3 += 1;
                    }
                }

                decimal tongket = (tongHs1 + tongHs2 * 2 + tongHs3 * 3) / ((tongsl1 + tongsl2 * 2 + tongsl3 * 3) == 0
                    ? 1
                    : (tongsl1 + tongsl2 * 2 + tongsl3 * 3));

                studentAverage.SubjectAverages.Add(new Tuple<string, double?>(subject!.NameSubject!, (double)tongket));
                processedSubjects.Add(subject.NameSubject!);
            }

            studentAverages.Add(studentAverage);
        }


        return View("ReportSemester", studentAverages);
    }

    public IActionResult ViewMonthsInSemester()
    {
        var Email = User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        var teacher = _teacherServices
            .GetAll()
            .FirstOrDefault(x => x.Email == Email);

        if (teacher == null)
        {
            _notyfService.Warning("Giáo viên không tồn tại hoặc không có quyền truy cập.");
            return RedirectToAction("IndexTeacher");
        }

        DateTime currentDate = DateTime.Now;

        var currentSemester = _semesterServices
            .GetAll()
            .OrderByDescending(s => s.StartIn)
            .FirstOrDefault(s => s.StartIn <= currentDate && s.EndIn >= currentDate)!;

        if (currentSemester == null)
        {
            _notyfService.Warning("Hiện không có học kỳ nào diễn ra.");
            return RedirectToAction("IndexLayout","Home");
        }

        var monthsInSemester = GetMonthsInSemester(currentSemester);

        return View(monthsInSemester);
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

    // điểm tổng kết hằng tháng của giáo viên
    public async Task<IActionResult> MonthTranscripts(int selectedMonth)
    {
        Teacher? teacher = null;
        Guid userId;
        try
        {
            teacher = await _teacherServices.GetElementAsync(
                x => x.Email!.ToLower() == HttpContext.Session.GetString("UserEmail")!.ToLower());
        }
        catch
        {
            userId = Guid.Empty; 
        }

        if (teacher == null)
        {
            //khong tim thay gv
            return RedirectToAction("ViewMonthsInSemester");
        }

        var thisClassroom = await _classroomServices.GetElementAsync(x => !x.IsDeleted && x.TeacherId == teacher.Id);

        if (thisClassroom == null)
        {
            _notyfService.Warning("Không tìm thấy lớp học" + $"{thisClassroom.Id}");
            return RedirectToAction("ViewMonthsInSemester");
        }


        var now = DateTime.Now;
        var semester = _semesterServices
            .GetAll()
            .Where(x => x.StartIn != null && x.EndIn != null)
            .FirstOrDefault(x => x.StartIn!.Value <= now && now <= x.EndIn!.Value);

        var listStudent = await _studentServices.GetAll()
            .Where(x => x.ClassRoomId == thisClassroom.Id)
            .ToListAsync();

        var listSubject = await _scheduleServices.GetAll()
            .Include(x => x.Subject)
            .Where(x => x.SemesterId == semester!.Id && x.ClassRoomId == thisClassroom.Id)
            .Select(x => x.Subject)
            .ToListAsync();

        var studentAverages = new List<StudentAverageDTO>();

        foreach (var student in listStudent)
        {
            var studentAverage = new StudentAverageDTO
            {    
                StudentName = $"{student.FirstName} {student.LastName}",
                SubjectAverages = new List<Tuple<string, double?>>()
            };

            var processedSubjects = new List<string>();

            foreach (var subject in listSubject)
            {
                if (processedSubjects.Contains(subject!.NameSubject!))
                    continue;

                var transcripts = await _transcriptServices.GetAll()
                    .Where(t => t.StudentId == student.Id
                                && t.SubjectId == subject!.Id
                                && t.SemesterId == semester!.Id
                                && t.Month == selectedMonth)
                    .ToListAsync();

                decimal tongHs1 = 0;
                decimal tongHs2 = 0;
                int tongsl1 = 0;
                int tongsl2 = 0;

                foreach (var studentTr in transcripts)
                {
                    if (studentTr.TypeId != 4)
                    {
                        if (studentTr.TypeId == 1 || studentTr.TypeId == 2)
                        {
                            tongHs1 += studentTr.Value;
                            tongsl1 += 1;
                        }
                        else
                        {
                            tongHs2 += studentTr.Value;
                            tongsl2 += 1;
                        }
                    }
                }

                decimal tongket = (tongHs1 + tongHs2 * 2) /
                                  ((tongsl1 + tongsl2 * 2) == 0 ? 1 : (tongsl1 + tongsl2 * 2));

                studentAverage.SubjectAverages.Add(new Tuple<string, double?>(subject!.NameSubject!, (double)tongket));
                processedSubjects.Add(subject.NameSubject!);
            }

            studentAverages.Add(studentAverage);
        }

        ViewBag.ClassroomId = thisClassroom.Id;
        ViewBag.SelectedMonth = selectedMonth;

        return View("MonthTranscripts", studentAverages);
    }


    // Phương thức để lấy danh sách học sinh trong một lớp
    private List<Student> GetStudentsInClass(Guid classRoomId)
    {
        return _studentServices
            .GetAll()
            .Include(student => student.ClassRoom)
            .Include(student => student.Transcripts)
            .Where(student => student.ClassRoomId == classRoomId)
            .OrderBy(student => student.Id)
            .ToList(); 
    }

    //Ràng buộc điểm trên 0 dưới 10
    private bool Valid(Transcript? t)
    {
        return t?.Value is >= 0 and <= 10;
    }
}