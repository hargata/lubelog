using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Unauthorized()
        {
            if (!User.IsInRole("CookieAuth"))
            {
                Response.StatusCode = 403;
                return new EmptyResult();
            }
            return View("401");
        }
    }
}
