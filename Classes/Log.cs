using System;
using System.IO;

namespace EncryptedEmailBridge.Classes
{
    internal static class Log
    {
        public static string message = string.Empty;

        public static void Add(string logMessage)
        {
            logMessage = logMessage.Replace(Environment.NewLine, " ");

            if (!string.IsNullOrEmpty(message))
            {
                message += Environment.NewLine;
            }

            message += logMessage;
        }

        public static bool Write()
        {
            try
            {
                string logFile = Const.RootPath + Const.LogDirName + "\\" + Const.CurrentDateStamp + ".log";

                Console.WriteLine(logFile);

                if (!File.Exists(logFile)) {
                    File.CreateText(logFile);
                }

                using (StreamWriter w = File.AppendText(logFile))
                {
                    w.WriteLine("Date : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (!string.IsNullOrEmpty(message))
                    {
                        w.WriteLine(message);
                    }
                    else
                    {
                        w.WriteLine("nothing to do");
                    }

                    w.WriteLine("---------------------------");
                }
                return true;
            }
            catch (Exception e)
            {
                Add("error writing to file: " + e);
                return false;
            }
        }
    }
}
