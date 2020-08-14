using Microsoft.AspNetCore.Mvc;
using ImageOptimizer.Models.HomeViewModels;

namespace ImageOptimizer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Pricing()
        {
            var model = new PricingViewModel {IsAuthenticated = HttpContext.User.Identity.IsAuthenticated };
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
            base.Dispose(disposing);
        }
    }
}
