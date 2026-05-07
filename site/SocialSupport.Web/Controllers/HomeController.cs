using Microsoft.AspNetCore.Mvc;

namespace SocialSupport.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult Download()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}