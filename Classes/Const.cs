using System;
using System.Configuration;

namespace EncryptedEmailBridge.Classes
{
    internal static class Const
    {
        public static string AppName = "EncryptedEmailBridge";
        public static string LogDirName = "log";
        public static string ArchiveDirName = "archive";
        public static string ProcessingDirName = "process";

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

        public static string CurrentDateStamp = DateTime.Now.ToString("yyyyMMdd");
        public static string CurrentTimeStamp = DateTime.Now.ToString("HHmmss_ffff");
        public static string CurrentDateTimeStamp = CurrentDateStamp + "_" + CurrentTimeStamp;
        public static int MaxLengthFileName = 128;

        public static bool FillVariables()
        {
            try
            {
                Method = ConfigurationManager.AppSettings["Method"];

                RootPath = ConfigurationManager.AppSettings["RootPath"];
                if (RootPath.Length > 3 && RootPath.Substring(RootPath.Length - 1, 1) != "\\") RootPath += "\\";
                Extension = ConfigurationManager.AppSettings["Extension"];

                EmailHost = ConfigurationManager.AppSettings["EmailHost"];
                Int32.TryParse(ConfigurationManager.AppSettings["EmailPort"], out EmailPort);
                string PortEncryption = ConfigurationManager.AppSettings["PortEncryption"];
                if (PortEncryption == "ssl" || PortEncryption == "starttls") Const.EmailPortEncryption = true;
                EmailSender = ConfigurationManager.AppSettings["EmailSender"];
                EmailRecipient = ConfigurationManager.AppSettings["EmailRecipient"];
                EmailUsername = ConfigurationManager.AppSettings["EmailUsername"];
                EmailPassword = ConfigurationManager.AppSettings["EmailPassword"];

                EncryptionSecret = ConfigurationManager.AppSettings["EncryptionSecret"];

                Int32.TryParse(ConfigurationManager.AppSettings["HistoryToKeepInDays"], out HistoryToKeepInDays);
                CleanUpLog = Convert.ToBoolean(ConfigurationManager.AppSettings["CleanUpLog"]);
                CleanUpArchive = Convert.ToBoolean(ConfigurationManager.AppSettings["CleanUpArchive"]);

                if (
                    !string.IsNullOrEmpty(Method) &&
                    !string.IsNullOrEmpty(RootPath) &&
                    !string.IsNullOrEmpty(Extension) &&
                    !string.IsNullOrEmpty(EmailHost) &&
                    EmailPort != 0 &&
                    !string.IsNullOrEmpty(PortEncryption) &&
                    !string.IsNullOrEmpty(EmailSender) &&
                    !string.IsNullOrEmpty(EmailRecipient) &&
                    !string.IsNullOrEmpty(EmailUsername) &&
                    !string.IsNullOrEmpty(EmailPassword) &&
                    !string.IsNullOrEmpty(EncryptionSecret) &&
                    HistoryToKeepInDays != 0)
                {
                    return true;
                }
                else
                {
                    Log.Add(
                        "Method: " + Method + " | " +
                        "RootPath: " + RootPath + " | " +
                        "Extension: " + Extension + " | " +
                        "EmailHost: " + EmailHost + " | " +
                        "EmailPort: " + EmailPort + " | " +
                        "PortEncryption: " + PortEncryption + " | " +
                        "EmailSender: " + EmailSender + " | " +
                        "EmailRecipient: " + EmailRecipient + " | " +
                        "EmailUsername: " + EmailUsername + " | " +
                        "EmailPassword: " + EmailPassword.Length + " | " +
                        "EncryptionSecret: " + EncryptionSecret.Length + " | " +
                        "HistoryToKeepInDays: " + HistoryToKeepInDays + ""
                        );
                }
            }
            catch (Exception e)
            {
                Log.Add("configuration: error > " + e);
            }

            return false;
        }
    }
}

