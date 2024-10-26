using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace WrocWithoutQueueCheck
{
    public static class Config
    {
        public static Uri Url;
        public static TimeSpan DefaultTimeoutSeconds;
        public static bool BrowserHeadlessMode;
        public static int MaxNumberOfRunsPerBrowserSession;
        public static TimeSpan DelayBetweenRunsSeconds;
        public static string PlaceName;
        public static string DocumentType;
        public static int MaxNumberOfFutureMonthsToCheck;
        public static bool NotificationSoundOnError;
        public static bool NotificationSoundOnHit;
        public static bool NotificationEmails;
        public static int TimeBetweenNotificationEmailWithTheSameHitsMinutes;
        public static int TimeBetweenSummaryEmailHours;

        public static string SmtpHost;
        public static int SmtpPort;
        public static string SmtpLogin;
        public static string SmtpPassword;
        public static bool SmtpUseDefaultCredentials;
        public static bool SmtpEnableSsl;
        public static string SmtpNotificationReceiver;

        public static void SetConfig()
        {
            var appSettingsPath = "./appsettings.json";
            if (!File.Exists(appSettingsPath)) throw new Exception($"File `{appSettingsPath}` does not exist!");
            dynamic appSettingsFile = JObject.Parse(File.ReadAllText(appSettingsPath));

            Url = new Uri(appSettingsFile.Url.Value);
            DefaultTimeoutSeconds = TimeSpan.FromSeconds(appSettingsFile.DefaultTimeoutSeconds.Value);
            BrowserHeadlessMode = appSettingsFile.BrowserHeadlessMode.Value;
            MaxNumberOfRunsPerBrowserSession = (int)appSettingsFile.MaxNumberOfRunsPerBrowserSession.Value;
            DelayBetweenRunsSeconds = TimeSpan.FromSeconds(appSettingsFile.DelayBetweenRunsSeconds.Value);
            PlaceName = appSettingsFile.PlaceName.Value;
            DocumentType = appSettingsFile.DocumentType.Value;
            MaxNumberOfFutureMonthsToCheck = (int)appSettingsFile.MaxNumberOfFutureMonthsToCheck.Value;
            NotificationSoundOnError = appSettingsFile.NotificationSoundOnError.Value;
            NotificationSoundOnHit = appSettingsFile.NotificationSoundOnHit.Value;
            NotificationEmails = appSettingsFile.NotificationEmails.Value;
            TimeBetweenNotificationEmailWithTheSameHitsMinutes = (int)appSettingsFile.TimeBetweenNotificationEmailWithTheSameHitsMinutes.Value;
            TimeBetweenSummaryEmailHours = (int)appSettingsFile.TimeBetweenSummaryEmailHours.Value;

            if (NotificationEmails)
            {
                var smtpPath = "./smtp.json";
                dynamic smtpFile = JObject.Parse(File.ReadAllText(smtpPath));
                if (!File.Exists(appSettingsPath)) throw new Exception($"File `{smtpPath}` does not exist, even though email notifications are enabled!");

                SmtpHost = smtpFile.SmtpHost.Value;
                SmtpPort = (int)smtpFile.SmtpPort.Value;
                SmtpLogin = smtpFile.SmtpLogin.Value;
                SmtpPassword = smtpFile.SmtpPassword.Value;
                SmtpUseDefaultCredentials = smtpFile.SmtpUseDefaultCredentials.Value;
                SmtpEnableSsl = smtpFile.SmtpEnableSsl.Value;
                SmtpNotificationReceiver = smtpFile.SmtpNotificationReceiver.Value;
            }
        }
    }
}
