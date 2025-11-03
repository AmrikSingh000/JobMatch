using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;
using JobMatch.Models;
using JobMatch.Models.Enums;
using JobMatch.Models.ViewModels;

namespace JobMatch.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        
        private static readonly int[] AllowedPageSizes = new[] { 10, 20, 50 };

        public JobsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] JobFilterVM f)
        {
            var query = _context.Jobs.Where(j => j.IsActive);

            if (!string.IsNullOrWhiteSpace(f.Q))
            {
                var q = f.Q.Trim();
                query = query.Where(j =>
                    EF.Functions.Like(j.Title, $"%{q}%") ||
                    EF.Functions.Like(j.Organization, $"%{q}%"));
            }

            if (!string.IsNullOrWhiteSpace(f.Location))
            {
                var city = f.Location.Trim();
                query = query.Where(j => EF.Functions.Like(j.Location, $"%{city}%"));
            }

            if (f.Type.HasValue)
            {
                query = query.Where(j => j.EmploymentType == f.Type.Value);
            }

            query = f.Sort == "Oldest"
                ? query.OrderBy(j => j.CreatedAt)
                : query.OrderByDescending(j => j.CreatedAt);

            
            f.Page = f.Page <= 0 ? 1 : f.Page;
            f.PageSize = AllowedPageSizes.Contains(f.PageSize) ? f.PageSize : 10;

            f.Total = await query.CountAsync();
            var skip = (f.Page - 1) * f.PageSize;
            f.Results = await query.Skip(skip).Take(f.PageSize).ToListAsync();

            
            f.Chips.Clear();
            if (!string.IsNullOrWhiteSpace(f.Q))
                f.Chips.Add(new FilterChip { Key = "Q", Value = f.Q!, Label = $"What: {f.Q}" });
            if (!string.IsNullOrWhiteSpace(f.Location))
                f.Chips.Add(new FilterChip { Key = "Location", Value = f.Location!, Label = $"Where: {f.Location}" });
            if (f.Type.HasValue)
                f.Chips.Add(new FilterChip { Key = "Type", Value = f.Type.ToString()!, Label = $"Type: {f.Type}" });
            if (f.Sort != "Newest")
                f.Chips.Add(new FilterChip { Key = "Sort", Value = f.Sort, Label = $"Sort: {f.Sort}" });

            return View(f);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (job == null) return NotFound();
            return View(job);
        }

        
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public IActionResult Create()
        {
            return View(new Job());
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job model)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.UtcNow;
            model.IsActive = true;
            model.PostedByUserId = _userManager.GetUserId(User) ?? "unknown";

            _context.Jobs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            return View(job);
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var entity = await _context.Jobs.FindAsync(id);
            if (entity == null) return NotFound();

            entity.Title = model.Title;
            entity.Organization = model.Organization;
            entity.Description = model.Description;
            entity.Location = model.Location;
            entity.JobType = model.JobType; 
            entity.SalaryRange = model.SalaryRange;
            entity.TagsCsv = model.TagsCsv;
            entity.EmploymentType = model.EmploymentType;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = entity.Id });
        }

        
        [HttpPost]
        [Authorize(Roles = "Recruiter,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();
            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        
        
        
        [HttpGet]
        [Authorize(Roles = "Recruiter,Admin")]
        public async Task<IActionResult> MyPosts(int page = 1, int pageSize = 10)
        {
            page = page <= 0 ? 1 : page;
            pageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

            var userId = _userManager.GetUserId(User) ?? "";
            var query = _context.Jobs
                .Where(j => j.PostedByUserId == userId)
                .OrderByDescending(j => j.CreatedAt);

            var total = await query.CountAsync();
            var results = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            
            var jobIds = results.Select(r => r.Id).ToList();
            var counts = await _context.JobApplications
                .Where(a => jobIds.Contains(a.JobId))
                .GroupBy(a => a.JobId)
                .Select(g => new { JobId = g.Key, C = g.Count() })
                .ToListAsync();
            ViewBag.ApplicantCounts = counts.ToDictionary(x => x.JobId, x => x.C);

            return View(results);
        }
    }
}
