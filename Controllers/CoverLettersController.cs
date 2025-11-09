using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using JobMatch.Models;
using JobMatch.Data;
using JobMatch.Models.ViewModels;
using JobMatch.Services.CoverLetters;

namespace JobMatch.Controllers
{
    [Authorize(Roles = "Jobseeker")]
    public class CoverLettersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICoverLetterGenerator _generator;

        public CoverLettersController(ApplicationDbContext db, ICoverLetterGenerator generator)
        {
            _db = db;
            _generator = generator;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? jobId = null, int? resumeId = null)
        {
            var vm = new CoverLetterRequest();
            if (jobId is not null)
            {
                var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
                if (job != null)
                {
                    vm.JobId = job.Id;
                    vm.JobTitle = job.Title;
                    vm.Company = job.Organization;
                    vm.JobDescription = job.Description;
                }
            }
            if (resumeId is not null)
            {
                var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId);
                vm.ResumeId = resume?.Id;
                vm.Resume = resume;
            }

            ViewBag.Resumes = await _db.Resumes.OrderByDescending(r => r.UploadedAt).ToListAsync();
            ViewBag.Jobs = await _db.Jobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoverLetterRequest request)
        {
            if (request.ResumeId is not null && request.Resume is null)
            {
                var r = await _db.Resumes.FirstOrDefaultAsync(x => x.Id == request.ResumeId);
                request.Resume = r;
            }

            var letter = _generator.Generate(request);
            request.GeneratedLetter = letter;

            ViewBag.Resumes = await _db.Resumes.OrderByDescending(r => r.UploadedAt).ToListAsync();
            ViewBag.Jobs = await _db.Jobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
            return View("Preview", request);
        }
    }
}
