using CarCareTracker.Models;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace CarCareTracker.Helper
{
    public interface IMailHelper
    {
        OperationResponse NotifyUserForRegistration(string emailAddress, string token);
        OperationResponse NotifyUserForPasswordReset(string emailAddress, string token);
        OperationResponse NotifyUserForAccountUpdate(string emailAddress, string token);
        OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders);
        OperationResponse SendTestEmail(string emailAddress, MailConfig testMailConfig);
    }
    public class MailHelper : IMailHelper
    {
        private readonly MailConfig mailConfig;
        private readonly string serverLanguage;
        private readonly string serverDomain;
        private readonly IFileHelper _fileHelper;
        private readonly ITranslationHelper _translator;
        private readonly ILogger<MailHelper> _logger;
        public MailHelper(
            IConfigHelper config,
            IFileHelper fileHelper,
            ITranslationHelper translationHelper,
            ILogger<MailHelper> logger
            ) {
            //load mailConfig from Configuration
            mailConfig = config.GetMailConfig();
            serverLanguage = config.GetServerLanguage();
            serverDomain = config.GetServerDomain();
            _fileHelper = fileHelper;
            _translator = translationHelper;
            _logger = logger;
        }
        public OperationResponse NotifyUserForRegistration(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token)) {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Your Registration Token for LubeLogger");
            string tokenHtml = token;
            if (!string.IsNullOrWhiteSpace(serverDomain))
            {
                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                //construct registration URL.
                tokenHtml = $"<a href='{cleanedURL}/Login/Registration?email={emailAddress}&token={token}' target='_blank'>{token}</a>";
            }
            string emailBody = $"<span>{_translator.Translate(serverLanguage, "A token has been generated on your behalf, please complete your registration for LubeLogger using the token")}: {tokenHtml}</span>";
            var result = SendEmail(new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            } else
            {
                return OperationResponse.Failed();
            }
        }
        public OperationResponse NotifyUserForPasswordReset(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Your Password Reset Token for LubeLogger");
            string tokenHtml = token;
            if (!string.IsNullOrWhiteSpace(serverDomain))
            {
                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                //construct registration URL.
                tokenHtml = $"<a href='{cleanedURL}/Login/ResetPassword?email={emailAddress}&token={token}' target='_blank'>{token}</a>";
            }
            string emailBody = $"<span>{_translator.Translate(serverLanguage, "A token has been generated on your behalf, please reset your password for LubeLogger using the token")}: {tokenHtml}</span>";
            var result = SendEmail(new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            }
            else
            {
                return OperationResponse.Failed();
            }
        }
        public OperationResponse SendTestEmail(string emailAddress, MailConfig testMailConfig)
        {
            if (string.IsNullOrWhiteSpace(testMailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Test Email from LubeLogger");
            string emailBody = _translator.Translate(serverLanguage, "If you are seeing this email it means your SMTP configuration is functioning correctly");
            var result = SendEmail(testMailConfig, new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            }
            else
            {
                return OperationResponse.Failed();
            }
        }
        public OperationResponse NotifyUserForAccountUpdate(string emailAddress, string token)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Your User Account Update Token for LubeLogger");
            string emailBody = $"{_translator.Translate(serverLanguage, "A token has been generated on your behalf, please update your account for LubeLogger using the token")}: {token}";
            var result = SendEmail(new List<string> { emailAddress}, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            }
            else
            {
                return OperationResponse.Failed();
            }
        }
        public OperationResponse NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders)
        {
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (!emailAddresses.Any())
            {
                return OperationResponse.Failed("No recipients could be found");
            }
            if (!reminders.Any())
            {
                return OperationResponse.Failed("No reminders could be found");
            }
            //get email template, this file has to exist since it's a static file.
            var emailTemplatePath = _fileHelper.GetFullFilePath(StaticHelper.ReminderEmailTemplate);
            string emailSubject = $"{_translator.Translate(serverLanguage, "Vehicle Reminders From LubeLogger")} - {DateTime.Now.ToShortDateString()}";
            //construct html table.
            string emailBody = File.ReadAllText(emailTemplatePath);
            emailBody = emailBody.Replace("{VehicleInformation}", $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{StaticHelper.GetVehicleIdentifier(vehicle)}");
            string tableHeader = $"<th>{_translator.Translate(serverLanguage, "Urgency")}</th><th>{_translator.Translate(serverLanguage, "Description")}</th><th>{_translator.Translate(serverLanguage, "Due")}</th>";
            string tableBody = "";
            foreach(ReminderRecordViewModel reminder in reminders)
            {
                var dueOn = reminder.Metric == ReminderMetric.Both ? $"{reminder.Date.ToShortDateString()} or {reminder.Mileage}" : reminder.Metric == ReminderMetric.Date ? $"{reminder.Date.ToShortDateString()}" : $"{reminder.Mileage}";
                tableBody += $"<tr class='{reminder.Urgency}'><td>{_translator.Translate(serverLanguage, StaticHelper.GetTitleCaseReminderUrgency(reminder.Urgency))}</td><td>{reminder.Description}</td><td>{dueOn}</td></tr>";
            }
            emailBody = emailBody.Replace("{TableHeader}", tableHeader).Replace("{TableBody}", tableBody);
            try
            {
                var result = SendEmail(emailAddresses, emailSubject, emailBody);
                if (result)
                {
                    return OperationResponse.Succeed("Email Sent!");
                } else
                {
                    return OperationResponse.Failed();
                }
            } catch (Exception ex)
            {
                return OperationResponse.Failed(ex.Message);
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

            var builder = new BodyBuilder();

            builder.HtmlBody = emailBody;

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
        private bool SendEmail(MailConfig testMailConfig, List<string> emailTo, string emailSubject, string emailBody)
        {
            string from = testMailConfig.EmailFrom;
            var server = testMailConfig.EmailServer;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            foreach (string emailRecipient in emailTo)
            {
                message.To.Add(new MailboxAddress(emailRecipient, emailRecipient));
            }
            message.Subject = emailSubject;

            var builder = new BodyBuilder();

            builder.HtmlBody = emailBody;

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(server, testMailConfig.Port, SecureSocketOptions.Auto);
                //perform authentication if either username or password is provided.
                //do not perform authentication if neither are provided.
                if (!string.IsNullOrWhiteSpace(testMailConfig.Username) || !string.IsNullOrWhiteSpace(testMailConfig.Password))
                {
                    client.Authenticate(testMailConfig.Username, testMailConfig.Password);
                }
                try
                {
                    client.Send(message);
                    client.Disconnect(true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return false;
                }
            }
        }
    }
}
