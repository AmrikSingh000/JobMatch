
using Microsoft.AspNetCore.Mvc;
namespace JobMatch.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Privacy() => View();
    }
}