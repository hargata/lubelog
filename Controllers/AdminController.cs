using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    [Authorize(Roles = nameof(UserData.IsAdmin))]
    public class AdminController : Controller
    {
        private ILoginLogic _loginLogic;
        public AdminController(ILoginLogic loginLogic)
        {
            _loginLogic = loginLogic;
        }
        public IActionResult Index()
        {
            var viewModel = new AdminViewModel
            {
                Users = _loginLogic.GetAllUsers(),
                Tokens = _loginLogic.GetAllTokens()
            };
            return View(viewModel);
        }
        public IActionResult GenerateNewToken(string emailAddress)
        {
            var result = _loginLogic.GenerateUserToken(emailAddress);
            return Json(result);
        }
        public IActionResult DeleteToken(int tokenId)
        {
            var result = _loginLogic.DeleteUserToken(tokenId);
            return Json(result);
        }
    }
}
