
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

using JobMatch.Data;
using JobMatch.Infrastructure;

namespace JobMatch.Controllers
{
    // Small POCO to persist data/privacy settings to App_Data/datapolicy.json
    public class DataPolicySettings
    {
        public int RetentionDays { get; set; } = 365;
        public bool AllowExportRequests { get; set; } = true;
        public bool AllowDeletionRequests { get; set; } = true;
        public string PrivacyNoticeText { get; set; } = "We store minimal data and allow export/delete on request.";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Authorize(Roles = "Admin")]
    public class AdminController(UserManager<IdentityUser> userManager, IHostEnvironment env, ApplicationDbContext db) : Controller
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly IHostEnvironment _env = env;
        private readonly ApplicationDbContext _db = db;

        // Lists users with their current role
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<(string Id, string Email, string Role)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // IdentityUser.Email is string? so coalesce to "" to satisfy (string Email)
                userRoles.Add((user.Id, user.Email ?? string.Empty, roles.FirstOrDefault() ?? "None"));
            }

            return View(userRoles);
        }

        // Change a user's role (inline from Users view)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, role);
            return RedirectToAction(nameof(Users));
        }

        // Shows tail of file-based audit log
        public IActionResult Audit()
        {
            var path = AuditLogger.GetAuditLogPath(_env);
            if (!System.IO.File.Exists(path)) return View(Array.Empty<string>());

            var lines = System.IO.File.ReadAllLines(path);
            Array.Reverse(lines);
            return View(lines.Take(500).ToArray()); // latest 500
        }

        // Very simple system metrics pulled from DB
        public async Task<IActionResult> Metrics()
        {
            var model = new
            {
                Users = await _userManager.Users.CountAsync(),
                // Adjust these DbSet names if your context differs
                Jobs = await _db.Jobs.CountAsync(),
                Applications = await _db.JobApplications.CountAsync()
            };
            return View(model);
        }

        private string SettingsPath =>
            Path.Combine(_env.ContentRootPath, "App_Data", "datapolicy.json");

        // Show / edit privacy & retention settings
        public IActionResult Settings()
        {
            DataPolicySettings settings;

            if (System.IO.File.Exists(SettingsPath))
            {
                var json = System.IO.File.ReadAllText(SettingsPath);
                settings = System.Text.Json.JsonSerializer.Deserialize<DataPolicySettings>(json) ?? new DataPolicySettings();
            }
            else
            {
                settings = new DataPolicySettings();
            }

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(DataPolicySettings input)
        {
            input.UpdatedAt = DateTime.UtcNow;

            var dir = Path.GetDirectoryName(SettingsPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var json = System.Text.Json.JsonSerializer.Serialize(
                input,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            System.IO.File.WriteAllText(SettingsPath, json);
            TempData["Saved"] = "Settings saved.";
            return RedirectToAction(nameof(Settings));
        }
    }
}