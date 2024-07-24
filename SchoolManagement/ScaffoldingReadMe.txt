Scaffolding has generated all the files and added the required dependencies.

However the Application's Startup code may require additional changes for things to work end to end.
Add the following code to the Configure method in your Application's Startup class if not already done:

        app.UseEndpoints(endpoints =>
        {
          endpoints.MapControllerRoute(
            name : "areas",
            pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
          );
        });
        dotnet ef migrations add SchoolMgr -c IdentityManagementDbContext -o Data/Identity/Migrations --project SchoolManagement
        dotnet ef database update --context IdentityManagementDbContext --project SchoolManagement
        
        dotnet ef migrations add SchoolMgr -c SchoolManagementDbContext -o Data/SchoolManagement/Migrations --project SchoolManagement
        dotnet ef database update --context SchoolManagementDbContext --project SchoolManagement
        
        dotnet ef dbcontext optimize -o Data/SchoolManagement -c SchoolManagementDbContext --project SchoolManagement
        
        
     