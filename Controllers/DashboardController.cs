
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobMatch.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Jobseeker")]
        public IActionResult Jobseeker() => View();

        [Authorize(Roles = "Recruiter")]
        public IActionResult Recruiter() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => View();
    }
}