using Microsoft.AspNetCore.Mvc;

namespace Recipedia.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
