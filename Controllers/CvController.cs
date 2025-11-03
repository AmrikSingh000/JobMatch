using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobMatch.Data;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Jobseeker,Admin")]
    public class CvController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CvController(IWebHostEnvironment env, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _env = env;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Msg"] = "Please choose a file.";
                return View();
            }

            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            Directory.CreateDirectory(uploads);

            var userId = _userManager.GetUserId(User) ?? "anon";
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".bin";
            var fname = $"{userId}_{DateTime.UtcNow.Ticks}{ext}";
            var path = Path.Combine(uploads, fname);
            using (var fs = System.IO.File.Create(path))
            {
                await file.CopyToAsync(fs);
            }

            TempData["Msg"] = $"Uploaded CV: {file.FileName}";
            return RedirectToAction(nameof(Upload));
        }
    }
}
