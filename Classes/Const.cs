using System;
using System.Configuration;

namespace EncryptedEmailBridge.Classes
{
    internal static class Const
    {
        public static string AppName = string.Empty;
        public static string Method = string.Empty;
        public static string RootPath = string.Empty;
        public static string Extension = string.Empty;

        public static string EmailHost = string.Empty;
        public static int EmailPort;
        public static bool EmailPortEncryption = false;
        public static string EmailSender = string.Empty;
        public static string EmailRecipient = string.Empty;
        public static string EmailUsername = string.Empty;
        public static string EmailPassword = string.Empty;

        public static string EncryptionSecret = string.Empty;
        public static int HistoryToKeepInDays;
        public static bool CleanUpLog;
        public static bool CleanUpArchive;
        public static string RelativeLogDir = string.Empty;
        public static string RelativeArchiveDir = string.Empty;
        public static string RelativeProcessingDir = string.Empty;

        public static string CurrentDateStamp = DateTime.Now.ToString("yyyyMMdd");
        public static string CurrentTimeStamp = DateTime.Now.ToString("HHmmss_ffff");
        public static string CurrentDateTimeStamp = CurrentDateStamp + "_" + CurrentTimeStamp;
        public static int MaxLengthFileName = 128;

        public static bool FillVariables()
        {
            try
            {
                AppName = ConfigurationManager.AppSettings["appName"];

                Method = ConfigurationManager.AppSettings["method"];
                RootPath = ConfigurationManager.AppSettings["rootPath"];
                Extension = ConfigurationManager.AppSettings["extension"];

                EmailHost = ConfigurationManager.AppSettings["server"];
                Int32.TryParse(ConfigurationManager.AppSettings["port"], out EmailPort);
                string portEncryption = ConfigurationManager.AppSettings["portEncryption"];
                if (portEncryption == "ssl" || portEncryption == "starttls") Const.EmailPortEncryption = true;
                EmailSender = ConfigurationManager.AppSettings["sender"];
                EmailRecipient = ConfigurationManager.AppSettings["recipient"];
                EmailUsername = ConfigurationManager.AppSettings["username"];
                EmailPassword = ConfigurationManager.AppSettings["password"];

                EncryptionSecret = ConfigurationManager.AppSettings["secret"];

                Int32.TryParse(ConfigurationManager.AppSettings["history"], out HistoryToKeepInDays);
                CleanUpLog = Convert.ToBoolean(ConfigurationManager.AppSettings["cleanUpLog"]);
                CleanUpArchive = Convert.ToBoolean(ConfigurationManager.AppSettings["cleanUpArchive"]);
                RelativeLogDir = ConfigurationManager.AppSettings["logDir"];
                RelativeArchiveDir = ConfigurationManager.AppSettings["archiveDir"];
                RelativeProcessingDir = ConfigurationManager.AppSettings["processingDir"];

                if (
                    !string.IsNullOrEmpty(Method) &&
                    !string.IsNullOrEmpty(RootPath) &&
                    !string.IsNullOrEmpty(Extension) &&
                    !string.IsNullOrEmpty(EmailHost) &&
                    EmailPort != 0 &&
                    !string.IsNullOrEmpty(portEncryption) &&
                    !string.IsNullOrEmpty(EmailSender) &&
                    !string.IsNullOrEmpty(EmailRecipient) &&
                    !string.IsNullOrEmpty(EmailUsername) &&
                    !string.IsNullOrEmpty(EmailPassword) &&
                    !string.IsNullOrEmpty(EncryptionSecret) &&
                    HistoryToKeepInDays != 0)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.AddLog("configuration: error > " + e);
            }
            return false;
        }
    }
}

