using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;
using JobMatch.Models;
using JobMatch.Services.Email;

namespace JobMatch.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAppEmailSender _email;

        public ApplicationsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAppEmailSender email)
        {
            _context = context;
            _userManager = userManager;
            _email = email; 
        }

        
        [HttpPost]
        [Authorize(Roles = "Jobseeker,Admin,Recruiter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, string? coverLetter) 
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
            if (job == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;

            var exists = await _context.JobApplications
                .AnyAsync(a => a.JobId == id && a.ApplicantUserId == userId);

            if (exists)
            {
                TempData["Msg"] = "You have already applied to this job.";
                return RedirectToAction("Details", "Jobs", new { id });
            }

            
            var app = new JobApplication
            {
                JobId = id,
                ApplicantUserId = userId
                
            };

            _context.JobApplications.Add(app);
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Application submitted.";
            return RedirectToAction(nameof(My));
        }

        
        [HttpGet]
        [Authorize(Roles = "Jobseeker,Admin,Recruiter")]
        public async Task<IActionResult> My()
        {
            var userId = _userManager.GetUserId(User)!;

            var list = await _context.JobApplications
                .Include(a => a.Job)
                .Where(a => a.ApplicantUserId == userId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(list);
        }

        
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> ForJob(int id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);
            if (job == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User) ?? "";
            var isOwner = job.PostedByUserId == currentUserId;
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (!isOwner && !isAdmin) return Forbid();

            var apps = await _context.JobApplications
                .Where(a => a.JobId == id)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            
            var userIds = apps.Select(a => a.ApplicantUserId).Distinct().ToList();
            var users = _context.Users.Where(u => userIds.Contains(u.Id))
                                      .ToDictionary(u => u.Id, u => u.Email);

            ViewBag.Job = job;
            ViewBag.UserEmails = users;

            return View(apps);
        }

        
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status)
        {
            var app = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (app == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User) ?? "";
            var isOwner = app.Job.PostedByUserId == currentUserId;
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (!isOwner && !isAdmin) return Forbid();

            app.Status = status;
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Application status updated.";

            
            try
            {
                var applicant = await _userManager.FindByIdAsync(app.ApplicantUserId);
                if (!string.IsNullOrWhiteSpace(applicant?.Email))
                {
                    var subj = $"Your application for '{app.Job.Title}' is now {app.Status}";
                    var body = $"<p>Hi,</p><p>Your application for <strong>{app.Job.Title}</strong> at <strong>{app.Job.Organization}</strong> is now <strong>{app.Status}</strong>.</p>";
                    await _email.SendAsync(applicant.Email!, subj, body);
                }
            }
            catch
            {
                
            }

            return RedirectToAction(nameof(ForJob), new { id = app.JobId });
        }
    }
}
