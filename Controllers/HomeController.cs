using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MDS_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;

        public HomeController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public IActionResult Index()
        {
            ViewBag.MapsKey = configuration["GoogleMaps:ApiKey"];
            return View();
        }
    }
}
