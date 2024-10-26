using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace WrocWithoutQueueCheck.Helpers
{
    public class SmtpHelper
    {
        private readonly SmtpClient SmtpClient;

        public SmtpHelper()
        {
            SmtpClient = new SmtpClient(Config.SmtpHost, Config.SmtpPort);
            SmtpClient.UseDefaultCredentials = Config.SmtpUseDefaultCredentials;
            SmtpClient.Credentials = new NetworkCredential(Config.SmtpLogin, Config.SmtpPassword);
            SmtpClient.EnableSsl = Config.SmtpEnableSsl;
        }

        public void SendEmail(string subject, string body, IEnumerable<Attachment> attachments)
        {
            var message = new MailMessage($"{Assembly.GetCallingAssembly().GetName().Name}@{Config.SmtpHost}", Config.SmtpNotificationReceiver, subject, body);
            foreach (var attachment in attachments) message.Attachments.Add(attachment);
            SmtpClient.Send(message);

            // Without it, file remains locked sometimes
            foreach (var attachment in attachments) attachment.Dispose();
        }
    }
}
