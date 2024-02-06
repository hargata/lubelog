using CarCareTracker.Models;
using System.Net.Mail;
using System.Net;

namespace CarCareTracker.Helper
{
    public interface IMailHelper
    {
        OperationResponse NotifyUserForRegistration(string emailAddress, string token);
        OperationResponse NotifyUserForPasswordReset(string emailAddress, string token);
        OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders);
    }
    public class MailHelper : IMailHelper
    {
        private readonly MailConfig mailConfig;
        private readonly IFileHelper _fileHelper;
        public MailHelper(
            IConfiguration config,
            IFileHelper fileHelper
            ) {
            //load mailConfig from Configuration
            mailConfig = config.GetSection("MailConfig").Get<MailConfig>();
            _fileHelper = fileHelper;
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
        public OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return new OperationResponse { Success = false, Message = "SMTP Server Not Setup" };
            }
            if (!emailAddresses.Any())
            {
                return new OperationResponse { Success = false, Message = "No recipients could be found" };
            }
            if (!reminders.Any())
            {
                return new OperationResponse { Success = false, Message = "No reminders could be found" };
            }
            //get email template, this file has to exist since it's a static file.
            var emailTemplatePath = _fileHelper.GetFullFilePath(StaticHelper.ReminderEmailTemplate);
            string emailSubject = $"Vehicle Reminders From LubeLogger - {DateTime.Now.ToShortDateString()}";
            //construct html table.
            string emailBody = File.ReadAllText(emailTemplatePath);
            emailBody = emailBody.Replace("{VehicleInformation}", $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{vehicle.LicensePlate}");
            string tableBody = "";
            foreach(ReminderRecordViewModel reminder in reminders)
            {
                var dueOn = reminder.Metric == ReminderMetric.Both ? $"{reminder.Date} or {reminder.Mileage}" : reminder.Metric == ReminderMetric.Date ? $"{reminder.Date.ToShortDateString()}" : $"{reminder.Mileage}";
                tableBody += $"<tr class='{reminder.Urgency}'><td>{StaticHelper.GetTitleCaseReminderUrgency(reminder.Urgency)}</td><td>{reminder.Description}</td><td>{dueOn}</td></tr>";
            }
            emailBody = emailBody.Replace("{TableBody}", tableBody);
            try
            {
                foreach (string emailAddress in emailAddresses)
                {
                    SendEmail(emailAddress, emailSubject, emailBody, true, true);
                }
                return new OperationResponse { Success = true, Message = "Email Sent!" };
            } catch (Exception ex)
            {
                return new OperationResponse { Success = false, Message = ex.Message };
            }
        }
        private bool SendEmail(string emailTo, string emailSubject, string emailBody, bool isBodyHtml = false, bool useAsync = false) {
            string to = emailTo;
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
            MailMessage message = new MailMessage(from, to);
            message.Subject = emailSubject;
            message.Body = emailBody;
            message.IsBodyHtml = isBodyHtml;
            SmtpClient client = new SmtpClient(server);
            client.EnableSsl = mailConfig.UseSSL;
            client.Port = mailConfig.Port;
            client.Credentials = new NetworkCredential(mailConfig.Username, mailConfig.Password);
            try
            {
                if (useAsync)
                {
                    client.SendMailAsync(message, new CancellationToken());
                }
                else
                {
                    client.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
