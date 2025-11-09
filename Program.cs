
using JobMatch.Data;
using JobMatch.Data.Seed;
using JobMatch.Infrastructure;


using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<JobMatch.Services.Email.IAppEmailSender, JobMatch.Services.Email.SmtpEmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();


builder.Services.AddScoped<JobMatch.Services.CoverLetters.ICoverLetterGenerator, JobMatch.Services.CoverLetters.SimpleCoverLetterGenerator>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Jobseeker", "Recruiter", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var adminEmail = "admin@jobmatch.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(adminUser, "TempPass!234");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}


try
{
    var appDataDir = Path.Combine(app.Environment.ContentRootPath, "App_Data");
    if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
}
catch { }

if (app.Environment.IsDevelopment())
{
    
app.UseMiddleware<AuditMiddleware>(); 
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.EnsureSeedAsync(scope.ServiceProvider);
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();