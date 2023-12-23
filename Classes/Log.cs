using System;
using System.IO;

namespace EncryptedEmailBridge.Classes
{
    internal static class Log
    {
        public static string log = string.Empty;

        public static void AddLog(string logMessage)
        {
            logMessage = logMessage.Replace(Environment.NewLine, " ");

            if (!string.IsNullOrEmpty(log))
            {
                log += Environment.NewLine;
            }

            log += logMessage;
        }

        public static bool WriteLog()
        {
            try
            {
                using (StreamWriter w = File.AppendText(Const.RootPath + Const.RelativeLogDir + "\\" + Const.CurrentDateStamp + ".log"))
                {
                    w.WriteLine("Date : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (!string.IsNullOrEmpty(log))
                    {
                        w.WriteLine(log);
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
                AddLog("error writing to file: " + e);
                return false;
            }
        }
    }
}
