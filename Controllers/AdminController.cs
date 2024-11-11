using CarCareTracker.Helper;
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
        private IUserLogic _userLogic;
        private IConfigHelper _configHelper;
        public AdminController(ILoginLogic loginLogic, IUserLogic userLogic, IConfigHelper configHelper)
        {
            _loginLogic = loginLogic;
            _userLogic = userLogic;
            _configHelper = configHelper;
        }
        public IActionResult Index()
        {
            var viewModel = new AdminViewModel
            {
                Users = _loginLogic.GetAllUsers().OrderBy(x=>x.Id).ToList(),
                Tokens = _loginLogic.GetAllTokens()
            };
            return View(viewModel);
        }
        public IActionResult GetTokenPartialView()
        {
            var viewModel = _loginLogic.GetAllTokens();
            return PartialView("_Tokens", viewModel);
        }
        public IActionResult GetUserPartialView()
        {
            var viewModel = _loginLogic.GetAllUsers().OrderBy(x => x.Id).ToList();
            return PartialView("_Users", viewModel);
        }
        public IActionResult GenerateNewToken(string emailAddress, bool autoNotify)
        {
            if (emailAddress.Contains(","))
            {
                string[] emailAddresses = emailAddress.Split(',');
                foreach(string emailAdd in emailAddresses)
                {
                    var trimmedEmail = emailAdd.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedEmail))
                    {
                        var result = _loginLogic.GenerateUserToken(emailAdd.Trim(), autoNotify);
                        if (!result.Success)
                        {
                            //if fail, return prematurely
                            return Json(result);
                        }
                    }
                }
                return Json(StaticHelper.GetOperationResponse(true, "Token Generated!"));
            } else
            {
                var result = _loginLogic.GenerateUserToken(emailAddress, autoNotify);
                return Json(result);
            }
        }
        [HttpPost]
        public IActionResult DeleteToken(int tokenId)
        {
            var result = _loginLogic.DeleteUserToken(tokenId);
            return Json(result);
        }
        [HttpPost]
        public IActionResult DeleteUser(int userId)
        {
            var result =_userLogic.DeleteAllAccessToUser(userId) && _configHelper.DeleteUserConfig(userId) && _loginLogic.DeleteUser(userId);
            return Json(result);
        }
        [HttpPost]
        public IActionResult UpdateUserAdminStatus(int userId, bool isAdmin)
        {
            var result = _loginLogic.MakeUserAdmin(userId, isAdmin);
            return Json(result);
        }
    }
}
