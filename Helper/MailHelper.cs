using CarCareTracker.Models;
using System.Net.Mail;
using System.Net;

namespace CarCareTracker.Helper
{
    public interface IMailHelper
    {
        OperationResponse NotifyUserForRegistration(string emailAddress, string token);
        OperationResponse NotifyUserForPasswordReset(string emailAddress, string token);
    }
    public class MailHelper : IMailHelper
    {
        private readonly MailConfig mailConfig;
        public MailHelper(
            IConfiguration config
            ) {
            //load mailConfig from Configuration
            mailConfig = config.GetSection("MailConfig").Get<MailConfig>();
        }
        public OperationResponse NotifyUserForRegistration(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return new OperationResponse { Success = false, Message = "SMTP Server Not Setup" };
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token)) {
                return new OperationResponse { Success = false, Message = "Email Address or Token is invalid" };
            }
            string emailSubject = "Your Registration Token for LubeLogger";
            string emailBody = $"A token has been generated on your behalf, please complete your registration for LubeLogger using the token: {token}";
            var result = SendEmail(emailAddress, emailSubject, emailBody);
            if (result)
            {
                return new OperationResponse { Success = true, Message = "Email Sent!" };
            } else
            {
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
        }
        public OperationResponse NotifyUserForPasswordReset(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return new OperationResponse { Success = false, Message = "SMTP Server Not Setup" };
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return new OperationResponse { Success = false, Message = "Email Address or Token is invalid" };
            }
            string emailSubject = "Your Password Reset Token for LubeLogger";
            string emailBody = $"A token has been generated on your behalf, please reset your password for LubeLogger using the token: {token}";
            var result = SendEmail(emailAddress, emailSubject, emailBody);
            if (result)
            {
                return new OperationResponse { Success = true, Message = "Email Sent!" };
            }
            else
            {
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
        }
        private bool SendEmail(string emailTo, string emailSubject, string emailBody) {
            string to = emailTo;
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
            MailMessage message = new MailMessage(from, to);
            message.Subject = emailSubject;
            message.Body = emailBody;
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
                return false;
            }
        }
    }
}
