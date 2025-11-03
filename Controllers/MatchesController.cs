using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Jobseeker,Admin")]
    public class MatchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatchesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var jobs = _context.Jobs.Where(j => j.IsActive);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                jobs = jobs.Where(j => EF.Functions.Like(j.Title, $"%{t}%") || EF.Functions.Like(j.TagsCsv, $"%{t}%") || EF.Functions.Like(j.Description, $"%{t}%"));
            }
            var list = await jobs.OrderByDescending(j => j.CreatedAt).Take(50).ToListAsync();
            ViewBag.Query = q ?? "";
            return View(list);
        }
    }
}
