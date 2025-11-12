using System;
using System.Configuration;

namespace EncryptedEmailBridge.Classes
{
    internal static class Const
    {
        public static string AppName { get; } = "EncryptedEmailBridge";
        public static string LogDirName { get; } = "log";
        public static string ArchiveDirName { get; } = "archive";
        public static string ProcessingDirName { get; } = "process";

        public static string Method { get; } = ConfigurationManager.AppSettings["Method"] ?? string.Empty;
        public static string RootPath { get; } = ConfigurationManager.AppSettings["RootPath"] ?? string.Empty;
        public static string Extension { get; } = ConfigurationManager.AppSettings["Extension"] ?? string.Empty;

        public static string EmailHost { get; } = ConfigurationManager.AppSettings["EmailHost"] ?? string.Empty;
        public static int EmailPort { get; } = int.TryParse(ConfigurationManager.AppSettings["EmailPort"], out var port) ? port : 0;
        public static bool EmailPortEncryption { get; } = (ConfigurationManager.AppSettings["EmailPortEncryption"] == "ssl" || ConfigurationManager.AppSettings["EmailPortEncryption"] == "starttls");
        public static string EmailSender { get; } = ConfigurationManager.AppSettings["EmailSender"] ?? string.Empty;
        public static string EmailRecipient { get; } = ConfigurationManager.AppSettings["EmailRecipient"] ?? string.Empty;
        public static string EmailUsername { get; } = ConfigurationManager.AppSettings["EmailUsername"] ?? string.Empty;
        public static string EmailPassword { get; } = ConfigurationManager.AppSettings["EmailPassword"] ?? string.Empty;

        public static string EncryptionSecret { get; } = ConfigurationManager.AppSettings["EncryptionSecret"] ?? string.Empty;
        public static int HistoryToKeepInDays { get; } = int.TryParse(ConfigurationManager.AppSettings["HistoryToKeepInDays"], out var historyDays) ? historyDays : 0;
        public static bool CleanUpLog { get; } = bool.TryParse(ConfigurationManager.AppSettings["CleanUpLog"], out var cleanLog) && cleanLog;
        public static bool CleanUpArchive { get; } = bool.TryParse(ConfigurationManager.AppSettings["CleanUpArchive"], out var cleanArchive) && cleanArchive;

        public static string CurrentDateStamp { get; } = DateTime.Now.ToString("yyyyMMdd");
        public static string CurrentTimeStamp { get; } = DateTime.Now.ToString("HHmmss_ffff");
        public static string CurrentDateTimeStamp { get; } = CurrentDateStamp + "_" + CurrentTimeStamp;
        public static int MaxLengthFileName { get; } = 128;

        public static bool VariablesFilled()
        {
            try
            {
                if (
                    !string.IsNullOrEmpty(Method) &&
                    !string.IsNullOrEmpty(RootPath) &&
                    !string.IsNullOrEmpty(Extension) &&
                    !string.IsNullOrEmpty(EmailHost) &&
                    EmailPort != 0 &&
                    !string.IsNullOrEmpty(EmailPortEncryption.ToString()) &&
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
                        "EmailPortEncryption: " + EmailPortEncryption + " | " +
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

