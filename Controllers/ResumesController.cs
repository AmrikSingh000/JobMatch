
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Jobseeker")]
    public class ResumesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        public ResumesController(IWebHostEnvironment env) => _env = env;

        // GET: /Resumes/Upload
        [HttpGet]
        public IActionResult Upload() => View();

        // POST: /Resumes/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose a file.");
                return View();
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            TempData["ResumeBytesLength"] = bytes.Length;
            return RedirectToAction(nameof(Matches));
        }

        // GET: /Resumes/Matches
        [HttpGet]
        public IActionResult Matches()
        {
            ViewBag.ResumeBytesLength = TempData["ResumeBytesLength"] ?? 0;
            return View();
        }
    }
}