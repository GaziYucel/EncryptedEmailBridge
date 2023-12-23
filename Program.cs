using EncryptedEmailBridge.Classes;
using System;

namespace EncryptedEmailBridge
{
    class Program
    {
        static void Main()
        {
            if (Const.FillVariables() && Util.CreateDirectories())
            {
                // inbound
                if (Const.Method == "out")
                {
                    OutBound outBound = new OutBound();
                    outBound.Execute();
                }

                // outbound
                if (Const.Method == "in")
                {
                    InBound inBound = new InBound();
                    inBound.Execute();
                }

                // cleanup log files older than HistoryToKeepInDays
                if (Const.CleanUpLog) Util.CleanupDirectory(Const.RootPath + Const.RelativeLogDir + "\\", "log");

                // cleanup archived files older than HistoryToKeepInDays
                if (Const.CleanUpArchive) Util.CleanupDirectory(Const.RootPath + Const.RelativeArchiveDir + "\\", Const.Extension);
                if (Const.CleanUpArchive) Util.CleanupDirectory(Const.RootPath + Const.RelativeArchiveDir + "\\", "eml");
            }

            if (!Log.WriteLog()) Console.WriteLine(Log.log);
        }
    }
}
