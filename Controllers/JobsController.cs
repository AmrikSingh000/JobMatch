
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;
using JobMatch.Models;
using System.Security.Claims;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public JobsController(ApplicationDbContext db) => _db = db;

        // Anyone can browse jobs
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? q = null)
        {
            var jobs = _db.Jobs.Where(j => j.IsActive);
            if (!string.IsNullOrWhiteSpace(q))
                jobs = jobs.Where(j =>
                    (j.Title ?? "").Contains(q) ||
                    (j.Organization ?? "").Contains(q) ||
                    (j.Location ?? "").Contains(q));

            var list = await jobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
            return View(list);
        }

        // GET: /Jobs/Create  (Recruiter only)
        public IActionResult Create() => View(new Job { IsActive = true });

        // POST: /Jobs/Create  (Recruiter only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Organization,Location,JobType,Description,IsActive")] Job job)
        {
            if (!ModelState.IsValid)
            {
                // Make validation visible in the view
                return View(job);
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid)) return Challenge();

            job.PostedByUserId = uid;
            job.CreatedAt = DateTime.UtcNow;

            _db.Jobs.Add(job);
            await _db.SaveChangesAsync();

            TempData["Flash"] = "Job posted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}