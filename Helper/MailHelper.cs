using CarCareTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CarCareTracker.Helper
{
    public interface IMailHelper
    {
        OperationResponse NotifyUserForRegistration(string emailAddress, string token);
        OperationResponse NotifyUserForPasswordReset(string emailAddress, string token);
        OperationResponse NotifyUserForAccountUpdate(string emailAddress, string token);
        OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders);
    }
    public class MailHelper : IMailHelper
    {
        private readonly MailConfig mailConfig;
        private readonly IFileHelper _fileHelper;
        private readonly ILogger<MailHelper> _logger;

        private const string MailServerNotSetUp = "SMTP Server Not Setup";
        private const string EmailAddressOrTokenInvalid = "Email Address or Token is invalid";
        private const string EmailSent = "Email Sent!";
        
        public MailHelper(
            IConfiguration config,
            IFileHelper fileHelper,
            ILogger<MailHelper> logger
            ) {
            //load mailConfig from Configuration
            mailConfig = config.GetSection("MailConfig").Get<MailConfig>() ?? new MailConfig();
            _fileHelper = fileHelper;
            _logger = logger;
        }
        public OperationResponse NotifyUserForRegistration(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return StaticHelper.GetOperationResponse(false, MailServerNotSetUp);
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return StaticHelper.GetOperationResponse(false, EmailAddressOrTokenInvalid);
            }
            const string emailSubject = "Your Registration Token for LubeLogger";
            string emailBody = $"A token has been generated on your behalf, please complete your registration for LubeLogger using the token: {token}";
            var result = SendEmail(new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, EmailSent);
            } else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
            }
        }
        public OperationResponse NotifyUserForPasswordReset(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return StaticHelper.GetOperationResponse(false, MailServerNotSetUp);
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return StaticHelper.GetOperationResponse(false, EmailAddressOrTokenInvalid);
            }
            const string emailSubject = "Your Password Reset Token for LubeLogger";
            string emailBody = $"A token has been generated on your behalf, please reset your password for LubeLogger using the token: {token}";
            var result = SendEmail(new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, EmailSent);
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
            }
        }
        public OperationResponse NotifyUserForAccountUpdate(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return StaticHelper.GetOperationResponse(false, MailServerNotSetUp);
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return StaticHelper.GetOperationResponse(false, EmailAddressOrTokenInvalid);
            }
            const string emailSubject = "Your User Account Update Token for LubeLogger";
            string emailBody = $"A token has been generated on your behalf, please update your account for LubeLogger using the token: {token}";
            var result = SendEmail(new List<string> { emailAddress}, emailSubject, emailBody);
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, EmailSent);
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
            }
        }
        public OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return StaticHelper.GetOperationResponse(false, MailServerNotSetUp);
            }
            if (emailAddresses.Count == 0)
            {
                return StaticHelper.GetOperationResponse(false, "No recipients could be found");
            }
            if (reminders.Count == 0)
            {
                return StaticHelper.GetOperationResponse(false, "No reminders could be found");
            }
            //get email template, this file has to exist since it's a static file.
            var emailTemplatePath = _fileHelper.GetFullFilePath(StaticHelper.ReminderEmailTemplate);
            string emailSubject = $"Vehicle Reminders From LubeLogger - {DateTime.Now.ToShortDateString()}";
            //construct html table.
            string emailBody = File.ReadAllText(emailTemplatePath);
            emailBody = emailBody.Replace("{VehicleInformation}", $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{StaticHelper.GetVehicleIdentifier(vehicle)}");
            string tableBody = "";
            foreach(ReminderRecordViewModel reminder in reminders)
            {
                var dueOn = reminder.Metric switch
                {
                    ReminderMetric.Both => $"{reminder.Date.ToShortDateString()} or {reminder.Mileage}",
                    ReminderMetric.Date => $"{reminder.Date.ToShortDateString()}",
                    _ => $"{reminder.Mileage}"
                };
                tableBody += $"<tr class='{reminder.Urgency}'><td>{StaticHelper.GetTitleCaseReminderUrgency(reminder.Urgency)}</td><td>{reminder.Description}</td><td>{dueOn}</td></tr>";
            }
            emailBody = emailBody.Replace("{TableBody}", tableBody);
            try
            {
                var result = SendEmail(emailAddresses, emailSubject, emailBody);
                if (result)
                {
                    return StaticHelper.GetOperationResponse(true, EmailSent);
                } else
                {
                    return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
                }
            } catch (Exception ex)
            {
                return StaticHelper.GetOperationResponse(false, ex.Message);
            }
        }
        private bool SendEmail(List<string> emailTo, string emailSubject, string emailBody) {
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            foreach(string emailRecipient in emailTo)
            {
                message.To.Add(new MailboxAddress(emailRecipient, emailRecipient));
            }
            message.Subject = emailSubject;

            var builder = new BodyBuilder
            {
                HtmlBody = emailBody
            };

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(server, mailConfig.Port, SecureSocketOptions.Auto);
                //perform authentication if either username or password is provided.
                //do not perform authentication if neither are provided.
                if (!string.IsNullOrWhiteSpace(mailConfig.Username) || !string.IsNullOrWhiteSpace(mailConfig.Password)) {
                    client.Authenticate(mailConfig.Username, mailConfig.Password);
                }
                try
                {
                    client.Send(message);
                    client.Disconnect(true);
                    return true;
                } catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return false;
                }
            }
        }
    }
}
