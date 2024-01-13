using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

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
            //send an email test block.
            SendEmail(emailAddress);
            return Json(result);
        }
        public IActionResult DeleteToken(int tokenId)
        {
            var result = _loginLogic.DeleteUserToken(tokenId);
            return Json(result);
        }
        private bool SendEmail(string emailAddress)
        {
            var mailConfig = new MailConfig();
            string to = emailAddress;
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Using the new SMTP client.";
            message.Body = @"Using this new feature, you can send an email message from an application very easily.";
            SmtpClient client = new SmtpClient(server);
            client.EnableSsl = mailConfig.UseSSL;
            client.Port = mailConfig.Port;
            client.Credentials = new NetworkCredential(mailConfig.Username, mailConfig.Password);

            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
                return false;
            }
        }
    }
}
