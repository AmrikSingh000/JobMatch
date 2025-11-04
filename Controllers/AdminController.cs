using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobMatch.Data;
using JobMatch.Models;
using JobMatch.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // For this student project we can keep settings in memory.
        // (In a real app this would be stored in the database or config.)
        private static DataPolicySettings _dataSettings = new DataPolicySettings();

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --------------------------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _userManager.Users.CountAsync();

            var jobseekers = await _userManager.GetUsersInRoleAsync("Jobseeker");
            var recruiters = await _userManager.GetUsersInRoleAsync("Recruiter");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var totalJobs = await _context.Jobs.CountAsync();
            var activeJobs = await _context.Jobs.CountAsync(j => j.IsActive);
            var totalApplications = await _context.JobApplications.CountAsync();

            var latestJobs = await _context.Jobs
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            var latestApplications = await _context.JobApplications
                .Include(a => a.Job)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(5)
                .ToListAsync();

            var vm = new AdminDashboardVM
            {
                TotalUsers = totalUsers,
                TotalJobseekers = jobseekers.Count,
                TotalRecruiters = recruiters.Count,
                TotalAdmins = admins.Count,
                TotalJobs = totalJobs,
                ActiveJobs = activeJobs,
                TotalApplications = totalApplications,
                LatestJobs = latestJobs,
                LatestApplications = latestApplications
            };

            return View(vm);
        }

        // --------------------------------------------------------------------
        // USER & ROLE MANAGEMENT
        // --------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var vmList = new List<AdminUserVM>();

            foreach (var u in users)
            {
                vmList.Add(new AdminUserVM
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    IsJobseeker = await _userManager.IsInRoleAsync(u, "Jobseeker"),
                    IsRecruiter = await _userManager.IsInRoleAsync(u, "Recruiter"),
                    IsAdmin = await _userManager.IsInRoleAsync(u, "Admin"),
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return View(vmList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(
            string id,
            bool isJobseeker,
            bool isRecruiter,
            bool isAdmin)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Ensure roles exist
            foreach (var roleName in new[] { "Jobseeker", "Recruiter", "Admin" })
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            await UpdateRole(user, "Jobseeker", isJobseeker);
            await UpdateRole(user, "Recruiter", isRecruiter);
            await UpdateRole(user, "Admin", isAdmin);

            TempData["Msg"] = "User roles updated.";
            return RedirectToAction(nameof(Users));
        }

        private async Task UpdateRole(IdentityUser user, string roleName, bool shouldHaveRole)
        {
            var currentlyInRole = await _userManager.IsInRoleAsync(user, roleName);

            if (shouldHaveRole && !currentlyInRole)
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
            else if (!shouldHaveRole && currentlyInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            await _userManager.UpdateAsync(user);

            TempData["Msg"] = "User locked.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            TempData["Msg"] = "User unlocked.";
            return RedirectToAction(nameof(Users));
        }

        // --------------------------------------------------------------------
        // ANNOUNCEMENTS (placeholder)
        // --------------------------------------------------------------------
        [HttpGet]
        public IActionResult Announcements()
        {
            return View();
        }

        // --------------------------------------------------------------------
        // DATA & PRIVACY SETTINGS
        // --------------------------------------------------------------------
        [HttpGet]
        public IActionResult Settings()
        {
            // For now this just returns the in-memory settings object.
            // In a real app you'd load from the database or configuration.
            return View(_dataSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(DataPolicySettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UpdatedAt = DateTime.UtcNow;
            _dataSettings = model;

            TempData["Saved"] = "Settings saved.";
            return View(_dataSettings);
        }
    }
}
