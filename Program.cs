using EncryptedEmailBridge.Classes;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EncryptedEmailBridge
{
    class Program
    {
        static void Main()
        {
            if (!Util.CheckRequirements())
            {
                Console.WriteLine(Log.message);
                return;
            }

            switch (Const.Method)
            {
                case "out":
                    OutBound outBound = new OutBound();
                    outBound.Execute();
                    break;
                case "in":
                    InBound inBound = new InBound();
                    inBound.Execute();
                    break;
                default:
                    break;
            }

            // cleanup log files older than HistoryToKeepInDays
            if (Const.CleanUpLog) Util.CleanupDirectory(Const.RootPath + Const.LogDirName + "\\", "log");

            // cleanup archived files older than HistoryToKeepInDays
            if (Const.CleanUpArchive) Util.CleanupDirectory(Const.RootPath + Const.ArchiveDirName + "\\", Const.Extension);
            if (Const.CleanUpArchive) Util.CleanupDirectory(Const.RootPath + Const.ArchiveDirName + "\\", "eml");

            Log.Write();
        }
    }
}
