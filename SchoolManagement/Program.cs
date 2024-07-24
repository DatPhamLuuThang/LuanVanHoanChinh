using System.Text.Encodings.Web;
using System.Text.Unicode;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using AutoMapper;
using CoreEntities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data.Identity;
using SchoolManagement.Data.SchoolManagement;
using SchoolManagement.Services;
using SchoolManagement.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));

builder.Services.AddSession();

//thêm authe,autho
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = "1016780812602-ms0p007ga0h5q1dt3vo0q0lq6m40l1ed.apps.googleusercontent.com";
        options.ClientSecret = "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";   
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policyBuilder => policyBuilder.RequireAssertion(context =>
            context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin")
        )
    );
    options.AddPolicy("Manager", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole("Manager")
        || context.User.IsInRole("Admin")));

    options.AddPolicy("Teacher", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole("Teacher")
        || context.User.IsInRole("Manager")
        || context.User.IsInRole("Admin")));

    options.AddPolicy("Student", policyBuilder => policyBuilder.RequireAssertion(context =>
        context.User.IsInRole("Student")
        || context.User.IsInRole("Teacher")
        || context.User.IsInRole("Admin")));

    options.AddPolicy("MoreRole", policyBuilder => policyBuilder.RequireAssertion(context =>
            context.User.Claims.Where(x => x.Type == "role").ToList().Count > 1 || context.User.IsInRole("SuperAdmin")
        )
    );
});
    
//cấu hình user trong Identity
builder.Services.AddIdentity<User, Role>(config =>
    {
        config.SignIn.RequireConfirmedAccount = false;
        // Thiết lập về Password
        config.Password.RequireDigit = false; // Không bắt phải có số
        config.Password.RequireLowercase = false; // Không bắt phải có chữ thường  
        config.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
        config.Password.RequireUppercase = false; // Không bắt buộc chữ in
        config.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
        config.Password.RequiredUniqueChars = 0; // Số ký tự riêng biệt

        // Cấu hình Lockout - khóa user
        config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Khóa 1 phút
        config.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa 1 phut
        config.Lockout.AllowedForNewUsers = true;

        // Cấu hình về User.
        config.User.AllowedUserNameCharacters = // các ký tự đặt tên user
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        config.User.RequireUniqueEmail = true; // Email là duy nhất

        // Cấu hình đăng nhập.
        config.SignIn.RequireConfirmedEmail = false; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
        config.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại
    })
    .AddEntityFrameworkStores<IdentityManagementDbContext>()
    .AddDefaultTokenProviders();

//cấu hình cookie
builder.Services.ConfigureApplicationCookie(config =>
{
    config.Cookie.Name = "SchoolManagement.Cookie";
    config.Cookie.SameSite = SameSiteMode.None;
    config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    config.ExpireTimeSpan = TimeSpan.FromHours(6);
    config.LoginPath = "/-/Identity-Login/Auth";
    config.LogoutPath = "/-/Identity-Logout/Auth";
    config.AccessDeniedPath = new PathString("/-/Identity-Error/Auth");
});

//thêm database
builder.Services.AddDbContextPool<IdentityManagementDbContext>((_, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityMngtConnection"));
});

builder.Services.AddDbContextPool<SchoolManagementDbContext>((_, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolManagementConnection"));
    options.UseModel(SchoolManagementDbContextModel.Instance);
});

//thêm service Identity
builder.Services.AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
    })
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(builder.Configuration.GetConnectionString("IdentityMngtConnection"));
    })
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext =
            b => 
                b.UseSqlServer(builder.Configuration.GetConnectionString("SchoolManagementConnection"));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(builder.Configuration.GetConnectionString("IdentityMngtConnection"));
    })
    .AddDeveloperSigningCredential()
    .AddAspNetIdentity<User>();

builder.Services.Configure<ExceptionHandlerOptions>(options =>
{
    options.AllowStatusCode404Response = true;
});

//thêm Service thông báo 
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 3;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopCenter;
    config.HasRippleEffect = true;
});



//Thêm Scope cho Servies
builder.Services.AddScoped(typeof(ICoreServices<>), typeof(CoreServices<>));
builder.Services.AddScoped(typeof(IMenuServices), typeof(MenuServices));
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddMvc();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseNotyf();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseCookiePolicy();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();


#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});
#pragma warning restore ASP0014


app.Run();