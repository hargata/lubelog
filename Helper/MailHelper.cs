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
        Task<OperationResponse> NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders);
        Task<OperationResponse> SendBackupEmail(string fileName, byte[] fileContent, string emailAddress);
        Task<OperationResponse> SendTestEmail(string emailAddress, MailConfig testMailConfig);
    }
    public class MailHelper : IMailHelper
    {
        private readonly IFileHelper _fileHelper;
        private readonly ITranslationHelper _translator;
        private readonly IConfigHelper _config;
        private readonly ILogger<MailHelper> _logger;
        public MailHelper(
            IConfigHelper config,
            IFileHelper fileHelper,
            ITranslationHelper translationHelper,
            ILogger<MailHelper> logger
            ) {
            _config = config;
            _fileHelper = fileHelper;
            _translator = translationHelper;
            _logger = logger;
        }
        public OperationResponse NotifyUserForRegistration(string emailAddress, string token)
        {
            //load mailConfig from Configuration
            var mailConfig = _config.GetMailConfig();
            var serverLanguage = _config.GetServerLanguage();
            var serverDomain = _config.GetServerDomain();
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token)) {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Your Registration Token for LubeLogger");
            //construct email body
            string emailBody = "<html><body style='font-family: arial, sans-serif;text-align: center;'>"; //begin
            emailBody += $"<span style='display:block;font-size:1.5em;font-weight:bold;padding:10px 15px;'>{emailSubject}</span>";
            emailBody += $"<span style='display:block;font-size:1.25em;padding:10px 15px;'>{_translator.Translate(serverLanguage, "A token has been generated on your behalf, please complete your registration for LubeLogger using the token")}</span>";
            emailBody += $"<span style='display:block;margin:10px;'><span style='border:2px dashed black;border-radius:12px;padding:10px 15px;font-family:Courier New, Courier, monospace;letter-spacing:6px;font-weight:bold;font-size:22px;'>{token}</span></span>";
            if (!string.IsNullOrWhiteSpace(serverDomain))
            {
                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                emailBody += $"<span style='display:block;margin-top:35px;margin-bottom:35px;'><a style='border-radius:12px;color:#fff;background-color:#0d6efd;padding:0.75rem 0.375rem;text-decoration:none;font-size:22px;' href='{cleanedURL}/Login/Registration?email={emailAddress}&token={token}' target='_blank'>{_translator.Translate(serverLanguage, "Register")}</a></span>";
            }
            emailBody += "</body></html>"; //end
            var result = SendEmail(mailConfig, new List<string> { emailAddress }, emailSubject, emailBody);
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
            //load mailConfig from Configuration
            var mailConfig = _config.GetMailConfig();
            var serverLanguage = _config.GetServerLanguage();
            var serverDomain = _config.GetServerDomain();
            if (string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                return OperationResponse.Failed("SMTP Server Not Setup");
            }
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(token))
            {
                return OperationResponse.Failed("Email Address or Token is invalid");
            }
            string emailSubject = _translator.Translate(serverLanguage, "Your Password Reset Token for LubeLogger");
            //construct email body
            string emailBody = "<html><body style='font-family: arial, sans-serif;text-align: center;'>"; //begin
            emailBody += $"<span style='display:block;font-size:1.5em;font-weight:bold;padding:10px 15px;'>{emailSubject}</span>";
            emailBody += $"<span style='display:block;font-size:1.25em;padding:10px 15px;'>{_translator.Translate(serverLanguage, "A token has been generated on your behalf, please reset your password for LubeLogger using the token")}</span>";
            emailBody += $"<span style='display:block;margin:10px;'><span style='border:2px dashed black;border-radius:12px;padding:10px 15px;font-family:Courier New, Courier, monospace;letter-spacing:6px;font-weight:bold;font-size:22px;'>{token}</span></span>";
            if (!string.IsNullOrWhiteSpace(serverDomain))
            {
                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                emailBody += $"<span style='display:block;margin-top:35px;margin-bottom:35px;'><a style='border-radius:12px;color:#fff;background-color:#0d6efd;padding:0.75rem 0.375rem;text-decoration:none;font-size:22px;' href='{cleanedURL}/Login/ResetPassword?email={emailAddress}&token={token}' target='_blank'>{_translator.Translate(serverLanguage, "Reset Password")}</a></span>";
            }
            emailBody += "</body></html>"; //end
            var result = SendEmail(mailConfig, new List<string> { emailAddress }, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            }
            else
            {
                return OperationResponse.Failed();
            }
        }
        public async Task<OperationResponse> SendTestEmail(string emailAddress, MailConfig testMailConfig)
        {
            //load mailConfig from Configuration
            var serverLanguage = _config.GetServerLanguage();
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
            var result = await SendEmailAsync(testMailConfig, new List<string> { emailAddress }, emailSubject, emailBody);
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
            //load mailConfig from Configuration
            var mailConfig = _config.GetMailConfig();
            var serverLanguage = _config.GetServerLanguage();
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
            var result = SendEmail(mailConfig, new List<string> { emailAddress}, emailSubject, emailBody);
            if (result)
            {
                return OperationResponse.Succeed("Email Sent!");
            }
            else
            {
                return OperationResponse.Failed();
            }
        }
        public async Task<OperationResponse> NotifyUserForReminders(Vehicle vehicle, List<string> emailAddresses, List<ReminderRecordViewModel> reminders)
        {
            //load mailConfig from Configuration
            var mailConfig = _config.GetMailConfig();
            var serverLanguage = _config.GetServerLanguage();
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
                var result = await SendEmailAsync(mailConfig, emailAddresses, emailSubject, emailBody);
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
        public async Task<OperationResponse> SendBackupEmail(string fileName, byte[] fileContent, string emailAddress)
        {
            //load mailConfig from Configuration
            var mailConfig = _config.GetMailConfig();
            var serverLanguage = _config.GetServerLanguage();
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(emailAddress, emailAddress));
            string emailSubject = $"{_translator.Translate(serverLanguage, "Automated Backup From LubeLogger")} - {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            message.Subject = emailSubject;
            //construct email body
            string emailBody = "<html><body style='font-family: arial, sans-serif;text-align: center;'>"; //begin
            emailBody += $"<span style='display:block;font-size:1.5em;font-weight:bold;padding:10px 15px;'>{emailSubject}</span>";
            emailBody += $"<span style='display:block;font-size:1.25em;padding:10px 15px;'>{_translator.Translate(serverLanguage, "Review the attached file containing a backup of your LubeLogger instance")}</span>";
            emailBody += "</body></html>"; //end
            var builder = new BodyBuilder();
            builder.HtmlBody = emailBody;
            builder.Attachments.Add(fileName, fileContent);
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(server, mailConfig.Port, SecureSocketOptions.Auto);
                //perform authentication if either username or password is provided.
                //do not perform authentication if neither are provided.
                if (!string.IsNullOrWhiteSpace(mailConfig.Username) || !string.IsNullOrWhiteSpace(mailConfig.Password))
                {
                    await client.AuthenticateAsync(mailConfig.Username, mailConfig.Password);
                }
                try
                {
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    return OperationResponse.Succeed("Backup Email Sent!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return OperationResponse.Failed();
                }
            }
        }
        private async Task<bool> SendEmailAsync(MailConfig mailConfig, List<string> emailTo, string emailSubject, string emailBody)
        {
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
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
                await client.ConnectAsync(server, mailConfig.Port, SecureSocketOptions.Auto);
                //perform authentication if either username or password is provided.
                //do not perform authentication if neither are provided.
                if (!string.IsNullOrWhiteSpace(mailConfig.Username) || !string.IsNullOrWhiteSpace(mailConfig.Password))
                {
                    await client.AuthenticateAsync(mailConfig.Username, mailConfig.Password);
                }
                try
                {
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return false;
                }
            }
        }
        private bool SendEmail(MailConfig mailConfig, List<string> emailTo, string emailSubject, string emailBody)
        {
            string from = mailConfig.EmailFrom;
            var server = mailConfig.EmailServer;
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
                client.Connect(server, mailConfig.Port, SecureSocketOptions.Auto);
                //perform authentication if either username or password is provided.
                //do not perform authentication if neither are provided.
                if (!string.IsNullOrWhiteSpace(mailConfig.Username) || !string.IsNullOrWhiteSpace(mailConfig.Password))
                {
                    client.Authenticate(mailConfig.Username, mailConfig.Password);
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
