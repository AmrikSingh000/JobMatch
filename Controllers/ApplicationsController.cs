// ApplicationsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;
using System.Security.Claims;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Jobseeker")]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ApplicationsController(ApplicationDbContext db) => _db = db;

        // GET: /Applications/Create?jobId=123
        [HttpGet]
        public async Task<IActionResult> Create(int? jobId)
        {
            if (jobId is null) return NotFound();
            var job = await _db.Jobs.FindAsync(jobId.Value);
            if (job == null) return NotFound();
            return View(job); // View is strongly-typed to Job and posts jobId
        }

        // POST: /Applications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int jobId)
        {
            var job = await _db.Jobs.FindAsync(jobId);
            if (job == null) return NotFound();

            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Forbid();

            var app = new JobMatch.Models.JobApplication
            {
                JobId = jobId,
                ApplicantUserId = uid,
                SubmittedAt = DateTime.UtcNow
            };

            _db.JobApplications.Add(app);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Mine));
        }

        // GET: /Applications/Mine
        [HttpGet]
        public async Task<IActionResult> Mine()
        {
            var uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(uid)) return Challenge();

            var apps = await _db.JobApplications
                .Include(a => a.Job)
                .Where(a => a.ApplicantUserId == uid)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(apps);
        }
    }
}