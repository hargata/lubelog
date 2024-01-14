using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Unauthorized()
        {
            return View("401");
        }
    }
}
