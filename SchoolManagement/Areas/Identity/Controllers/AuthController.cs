using System.Security.Claims;
using AspNetCoreHero.ToastNotification.Abstractions;
using CoreEntities.SchoolMgntModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Areas.Identity.Models;
using SchoolManagement.Data.Identity;
using SchoolManagement.Models;
using SchoolManagement.Services.Interface;
using Utilities;

namespace SchoolManagement.Areas.Identity.Controllers;

[Authorize(Policy = "Manager")]
[Area("Identity")]
[Route("-/[area]-[action]/[controller]")]
public class AuthController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;


    private readonly ICoreServices<Teacher> _teacherService;
    private readonly ICoreServices<Student> _studentService;

    private readonly IIdentityServerInteractionService _interactionService;
    private readonly IdentityManagementDbContext _appDbContext;
    private readonly INotyfService _notyfservice;

    public AuthController(SignInManager<User> signInManager, UserManager<User> userManager,
        IIdentityServerInteractionService interactionService, RoleManager<Role> roleManager,
        IdentityManagementDbContext appDbContext, INotyfService notyfservice,
        ICoreServices<Teacher> teacherService, ICoreServices<Student> studentService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _interactionService = interactionService;
        _roleManager = roleManager;
        _appDbContext = appDbContext;
        _notyfservice = notyfservice;
        _teacherService = teacherService;
        _studentService = studentService;
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("LoginWithGoogle")]
    public async Task LoginWithGoogle(string returnUrl)
    {
        try
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse", new { returnUrl })
            });
        }
        catch (Exception ex)
        {
            _notyfservice.Error(ex.Message);
            RedirectToAction("Login", new { returnUrl });
        }
    }

    [AllowAnonymous]
    public async Task<IActionResult> GoogleResponse(string returnUrl)
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (authenticateResult.Succeeded)
            {
                // Authentication succeeded, you can access the user information from the authenticateResult.Principal object

                var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);

                if (string.IsNullOrEmpty(email))
                {
                    _notyfservice.Error("Không tìm thấy email");
                    return RedirectToAction("Login", new { returnUrl });
                }

                var user = await ValidUser(email);

                if (user != null)
                {
                    // check if the password is correct
                    await _signInManager.SignInAsync(user, true);
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email!);
                    await _signInManager.RefreshSignInAsync(user);

                    // redirect to the return url
                    _notyfservice.Success("Đăng nhập thành công");

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Count == 0)
                    {
                        _notyfservice.Warning("Bạn chưa được cấp quyền");
                        return RedirectToAction("Error", "Auth", new { area = "Identity" });
                    }
                    else
                    {
                        if (roles.Count == 1)
                        {
                            return RedirectToAction("Index", "Home", new { area = roles.First() });
                        }
                        else
                        {
                            _notyfservice.Warning("Bạn được cấp 2 quyền trở lên");
                            return Redirect(returnUrl);
                        }
                    }
                }
                else
                {
                    if (!await CreateAuto(email))
                    {
                        _notyfservice.Error("Tài khoản đã bị khóa!");
                        return RedirectToAction("Login", new { returnUrl });
                    }

                    _notyfservice.Error("Chờ phê duyệt");
                    return RedirectToAction("Login", new { returnUrl });
                }
            }
            else
            {
                _notyfservice.Error("Đăng nhập hất bại");
                return RedirectToAction("Login", new { returnUrl });
            }
        }
        catch (Exception)
        {
            _notyfservice.Error("Lỗi");
            return RedirectToAction("Login", new { returnUrl });
        }
    }


    [AllowAnonymous]
    public IActionResult Error(string? msg)
    {
        msg ??= "Ohh!! Bạn chưa được cấp quyền để vào trang này!";

        ViewBag.msg = msg;
        return View();
    }


    //Đăng nhập
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        // get tenant info// check if the model is valid
        if (ModelState.IsValid)
        {
            // check if the user exists
            var user = await ValidUser(vm.Username);
            if (user != null)
            {
                // check if the password is correct
                var signInResult = _signInManager.PasswordSignInAsync(user, vm.Password, true, true).Result;

                if (signInResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    _notyfservice.Success("Đăng nhập thành công");

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Count == 0)
                    {
                        _notyfservice.Warning("Bạn chưa được cấp quyền");
                        return RedirectToAction("Error", "Auth", new { area = "Identity", returnUrl = vm.ReturnUrl });
                    }

                    return string.IsNullOrEmpty(vm.ReturnUrl)
                        ? RedirectToAction("Index", "Home", new { area = roles.First() })
                        : Redirect(vm.ReturnUrl);
                }
                else
                {
                    _notyfservice.Warning("Bạn nhập sai mật khẩu!");
                    return RedirectToAction("Login", "Auth", new { area = "Identity", returnUrl = vm.ReturnUrl });
                }
            }
            else
            {
                _notyfservice.Error("Bị khóa hoặc không tồn tại tài khoản");
                return RedirectToAction("Login", "Auth", new { area = "Identity", returnUrl = vm.ReturnUrl });
            }
        }

        _notyfservice.Warning("Kiểm tra lại thông tin");
        return RedirectToAction("Login", "Auth", new { area = "Identity", returnUrl = vm.ReturnUrl });
    }


    //Đăng xuất
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(string logoutId)
    {
        if (User.Identity?.Name != null)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                await _signInManager.SignOutAsync();

                _notyfservice.Success("Đăng xuất thành công");

                var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);

                if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                return Redirect(logoutRequest.PostLogoutRedirectUri);
            }
        }

        _notyfservice.Error("Xảy ra lỗi, vui lòng thử lại");
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    //Thêm tài khoản
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Sigup(string? returnUrl)
    {
        return View(new SigupViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Sigup(SigupViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            if (await _userManager.FindByEmailAsync(vm.Username) == null)
            {
                if (vm.IsTeacher)
                {
                    var user = new User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = vm.Username,
                        Email = vm.Username,
                        NormalizedEmail = vm.Username.ToUpper(),
                        NormalizedUserName = vm.Username.ToUpper(),
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        Active = true
                    };
                    var birthday = DateTime.Now.AddYears(-18);
                    var teacher = new Teacher
                    {
                        Id = Guid.NewGuid(),
                        Email = vm.Username,
                        CreatedBy = default,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        UpdatedBy = default,
                        UpdatedAt = DateTime.Now,
                        IsDeleted = false,
                        DeletedAt = default,
                        DeletedBy = default,
                        FirstName = vm.Username,
                        LastName = default,
                        PhoneNumber = default,
                        Address = default,
                        Gender = default,
                        BirthDay = birthday
                    };

                    var result = await _userManager.CreateAsync(user, vm.Password);

                    Helper.NotyfAssist(await _teacherService.AddAsync(teacher, teacher.Id), _notyfservice);

                    if (result.Succeeded)
                    {
                        _notyfservice.Success("Thành công");

                        if (vm.ReturnUrl != null)
                        {
                            return Redirect(vm.ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    var birthday = DateTime.Now.AddYears(-15);
                    var user = new User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = vm.Username,
                        Email = vm.Username,
                        NormalizedEmail = vm.Username.ToUpper(),
                        NormalizedUserName = vm.Username.ToUpper(),
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0,
                        Active = true
                    };

                    var student = new Student()
                    {
                        Id = Guid.NewGuid(),
                        Email = vm.Username,
                        CreatedBy = default,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = default,
                        IsDeleted = false,
                        IsActive = true,
                        DeletedAt = default,
                        DeletedBy = default,
                        FirstName = vm.Username,
                        LastName = default,
                        PhoneNumber = default,
                        Address = default,
                        Gender = default,
                        BirthDay = birthday,

                        ParentId = null,
                        ClassRoomId = null
                    };
                    Helper.NotyfAssist(await _studentService.AddAsync(student, student.Id), _notyfservice);

                    var result = await _userManager.CreateAsync(user, vm.Password);

                    if (result.Succeeded)
                    {
                        _notyfservice.Success("Thành công");

                        if (vm.ReturnUrl != null)
                        {
                            return Redirect(vm.ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var a = ex;
        }

        _notyfservice.Warning("Kiểm tra lại thông tin");
        return View(vm);
    }


    public IActionResult Dashboard()
    {
        _notyfservice.Information("Auth Area !");
        return View();
    }

    //Thêm quyền
    [HttpGet]
    public IActionResult RegisterRole(string returnUrl)
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterRole(RegisterRoleViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        if (await _roleManager.FindByNameAsync(vm.Name) == null)
        {
            var role = new Role(vm.Name);

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                _notyfservice.Success("Thành công");
                return RedirectToAction("GetAllRole", "Auth");
            }
        }

        return View(vm);
    }


    //Thêm quyền cho tài khoản
    [HttpGet]
    public IActionResult RegisterUserRole(string returnUrl)
    {
        ViewData["lsUser"] = new SelectList(_appDbContext.User, "Id", "UserName");
        ViewData["lsRole"] = new SelectList(_appDbContext.Role, "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUserRole(RegisterUserRoleViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var role = await _roleManager.FindByIdAsync(vm.RoleId);
        var user = await _userManager.FindByIdAsync(vm.UserId);


        if (user != null && role is { Name: not null })
        {
            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                _notyfservice.Success("Thành công");
                return RedirectToAction("Dashboard", "Auth");
            }
        }

        ViewData["lsUser"] = new SelectList(_appDbContext.User, "Id", "UserName");
        ViewData["lsRole"] = new SelectList(_appDbContext.Role, "Id", "Name");
        return View(vm);
    }

    //Đổi mật khẩu
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ChangePassword(string returnUrl)
    {
        return View(new ChangePassword() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ChangePassword(ChangePassword vm)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            _notyfservice.Warning("Người dùng không tồn tại");
            return View();
        }

        if (vm.NewPassword.Length < 10 || vm.NewPassword != vm.ConfirmPassword || vm.OldPassword == vm.NewPassword)
        {
            _notyfservice.Warning("Kiểm tra lại thông tin!");
            return View();
        }
        else
        {
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                _notyfservice.Error("Đổi mật khẩu không thành công. Vui lòng kiểm tra thông tin và thử lại.");
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);

            await _signInManager.SignOutAsync();


            _notyfservice.Success("Đổi mật khẩu thành công. Vui lòng đăng nhập lại");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [AllowAnonymous]
    private async Task<bool> CreateAuto(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        var user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0
        };

        var result = await _userManager.CreateAsync(user, email);

        return result.Succeeded;
    }


    //lấy danh sách người dùng
    [HttpGet]
    public IActionResult GetAllUsers(string returnUrl)
    {
        var data = _appDbContext
            .Users
            .ToList()
            .Select(x =>
            {
                var listUserRole = _appDbContext.UserRoles
                    .Where(ur => ur.UserId == x.Id)
                    .Select(ur => ur.RoleId)
                    .ToList();

                var listRoles = _appDbContext.Role
                    .Where(r => listUserRole.Contains(r.Id))
                    .ToList();

                return new UserWithRole
                {
                    User = x,
                    ListRole = listRoles
                };
            });

        ViewData["listRoles"] = new SelectList(_appDbContext.Role, "Id", "Name");
        return View(data);
    }

    //lấy danh sách quyền trong hệ thống
    [HttpGet]
    public IActionResult GetAllRole(string returnUrl)
    {
        var data = _appDbContext.Roles.ToList();

        return View(data);
    }


    //Thay đổi trạng thái truy cập
    [HttpGet]
    public async Task<IActionResult> ChangeActive(string id)
    {
        var account = await _userManager.FindByIdAsync(id);

        if (account != null)
        {
            account.Active = !account.Active;
            await _userManager.UpdateAsync(account);
        }
        else
        {
            _notyfservice.Error("Đã xảy ra lỗi hoặc không tìm thấy tài khoản");
            return RedirectToAction("GetAllUsers", "Auth");
        }

        _notyfservice.Success("Thành công");

        return RedirectToAction("GetAllUsers", "Auth");
    }

    private async Task<User?> ValidUser(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }

        try
        {
            var user = await _userManager.FindByNameAsync(username);

            return user is { Active: true } ? user : null;
        }
        catch
        {
            return null;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserRole(string id)
    {
        var account = await _userManager.FindByIdAsync(id);

        if (account == null)
        {
            _notyfservice.Error("Không tìm thấy User");
            return RedirectToAction("GetAllUsers");
        }

        var userRole = _appDbContext.UserRoles
            .Where(ur => ur.UserId == account.Id)
            .Select(ur => ur.RoleId)
            .ToList();

        var listSelectRoles = _appDbContext.Role
            .Where(r => !userRole.Contains(r.Id))
            .Select(x => new
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();

        var thisUserRole = _appDbContext.Role
            .Where(r => userRole.Contains(r.Id))
            .ToList();

        var un = account.UserName;
        if (un != null) ViewBag.name = un;

        ViewData["lsRole"] = new SelectList(listSelectRoles, "Id", "Name");

        return View(new DeleteRoleModel { User = account, listRole = thisUserRole });
    }

    [HttpPost]
    public async Task<IActionResult> AddUserRole(CreateUserRole data)
    {
        var role = await _roleManager.FindByIdAsync(data.IdRole);
        var user = await _userManager.FindByIdAsync(data.IdUser);


        if (user != null && role is { Name: not null })
        {
            var result = await _userManager.AddToRoleAsync(user, role.Name);

            data.nameUser = user.UserName;

            if (result.Succeeded)
            {
                _notyfservice.Success("Thành công");
                return RedirectToAction("GetUserRole", "Auth", new { id = data.IdUser });
            }
        }

        _notyfservice.Warning("Không hành công");
        return RedirectToAction("GetUserRole", "Auth", new { id = data.IdUser });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteUserRole(string idUser, string idRole)
    {
        var account = await _userManager.FindByIdAsync(idUser);

        var role = await _roleManager.FindByIdAsync(idRole);

        if (account != null && role is { Name: not null })
        {
            var check = _appDbContext.UserRoles
                .FirstOrDefault(x => x.RoleId == idRole && x.UserId == idUser);

            if (check == null)
            {
                _notyfservice.Warning("khong tim thay");
                return RedirectToAction("GetUserRole", new { id = account.Id });
            }


            _appDbContext.UserRoles.Remove(check);
            await _appDbContext.SaveChangesAsync();

            // await _userManager.RemoveFromRoleAsync(account, role.Name);
            _notyfservice.Success("Thành công");
        }

        return RedirectToAction("GetUserRole", new { id = idUser });
    }

    [HttpGet]
    public async Task<IActionResult> DeleteRole(string idRole)
    {
        var role = await _roleManager.FindByIdAsync(idRole);


        if (role is { Name: not null })
        {
            var data = _appDbContext.Roles
                .FirstOrDefault(x => x.Id == idRole);

            if (data == null)
            {
                _notyfservice.Warning("Không tìm thấy quyền");
                return RedirectToAction("GetAllRole", new { id = role.Id });
            }


            _appDbContext.Roles.Remove(data);

            await _appDbContext.SaveChangesAsync();

            _notyfservice.Success("Thành công");
        }

        return RedirectToAction("GetAllRole", "Auth");
    }


    [HttpGet]
    public async Task<IActionResult> DeleteAccount(string idAc)
    {
        var acc = await _userManager.FindByIdAsync(idAc);


        if (acc != null)
        {
            var data = _appDbContext.Users
                .FirstOrDefault(x => x.Id == idAc);

            if (data == null)
            {
                _notyfservice.Warning("Không tìm tài khoản");
                return RedirectToAction("GetAllUsers", new { id = acc.Id });
            }


            _appDbContext.Users.Remove(data);

            await _appDbContext.SaveChangesAsync();

            _notyfservice.Success("Thành công");
        }

        return RedirectToAction("GetAllUsers", "Auth");
    }
    

    [HttpGet]
    public IActionResult ResetUserPassword(string userId)
    {
        var newPassword = GenerateRandomPassword();
        var model = new ResetUserPasswordViewModel
        {
            UserId = userId,
            DisplayNewPassword = newPassword
        };

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> ResetUserPassword(ResetUserPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            
            if (user is { Active: true } )
            {
               
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.DisplayNewPassword);

                if (result.Succeeded)
                {
                    _notyfservice.Success($"Đã cấp lại mật khẩu mới cho tài khoản {user.UserName}. " +
                                          $"Mật khẩu mới là: {model.DisplayNewPassword}");
                    return RedirectToAction("GetAllUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng.");
            }
        }

        _notyfservice.Warning("Kiểm tra lại thông tin");
        return View(model);
    }
    
    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var newPassword = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return newPassword;
    }
    
}