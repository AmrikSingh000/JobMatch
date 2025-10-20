
using JobMatch.Data;
using JobMatch.Data.Seed;
using JobMatch.Infrastructure;
// this file is for app startup / DI / routing. just keeping it simple.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

// Setup builder (configuration, DI, logging)
var builder = WebApplication.CreateBuilder(args);
// --- Services (dependency injection) ---

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

// Build the app and prepare the HTTP pipeline
var app = builder.Build();

// apply EF migrations, ensure roles + default admin
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

// ensure App_Data exists (for audit/settings files)
try
{
    var appDataDir = Path.Combine(app.Environment.ContentRootPath, "App_Data");
    if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
}
catch { }

if (app.Environment.IsDevelopment())
{
    // --- Middleware
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

// --- Endpoints / routing ---
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();