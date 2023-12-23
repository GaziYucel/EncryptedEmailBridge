using System;
using System.IO;

namespace EncryptedEmailBridge.Classes
{
    internal static class Util
    {
        public static string ShortenFileName(string file)
        {
            string local = file;

            if (local.Length > Const.MaxLengthFileName)
            {
                int fileExtPos = local.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                if (fileExtPos >= 0)
                {
                    string name = local.Substring(0, fileExtPos);
                    string ext = local.Substring(fileExtPos + 1, local.Length - name.Length - 1);
                    name = name.Substring(0, Const.MaxLengthFileName - ext.Length - 1).Trim();
                    local = name + "." + ext;
                }
                else
                {
                    local = local.Substring(0, Const.MaxLengthFileName);
                }
                Log.AddLog("file name shortened: " + file + " > " + local);
            }

            return local;
        }

        public static bool CreateDirectories()
        {
            bool local = true;
            if (Const.RootPath.Length > 3 && Const.RootPath.Substring(Const.RootPath.Length - 1, 1) != "\\")
            {
                Const.RootPath += "\\";
            }
            string[] dirs = {
                Const.RootPath,
                Const.RootPath + Const.RelativeLogDir,
                Const.RootPath + Const.RelativeProcessingDir,
                Const.RootPath + Const.RelativeArchiveDir };

            foreach (string dir in dirs)
            {
                try
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        Log.AddLog("directory created: " + dir);
                    }
                }
                catch (Exception e)
                {
                    Log.AddLog("create directory error: " + dir + " > " + e);
                    local = false;
                }
            }
            return local;
        }

        public static string[] GetFileNamesInDirectory(string path, string extension)
        {
            string[] files = Directory.GetFiles(path, "*." + extension, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(path, "");
            }
            return files;
        }

        public static void ArchiveFiles(string path, string[] files)
        {
            foreach (string file in files)
            {
                try
                {
                    if (File.Exists(path + file))
                    {
                        File.Move(
                        path + file,
                            Const.RootPath + Const.RelativeArchiveDir + "\\" + ShortenFileName(Const.CurrentDateTimeStamp + "_" + file));

                    }
                    Log.AddLog("archived " + file + " > " + ShortenFileName(Const.CurrentDateTimeStamp + "_" + file));
                }
                catch (Exception e)
                {
                    Log.AddLog("error archiving " + file + ": " + e);
                }
            }
        }

        public static void DeleteFile(string filePath, string fileName)
        {
            try
            {
                if (File.Exists(filePath + fileName))
                {
                    File.Delete(filePath + fileName);
                    Log.AddLog("deleted " + filePath + fileName);
                }
                else
                {
                    Log.AddLog("not deleted (not found) " + filePath + fileName);
                }
            }
            catch (Exception e)
            {
                Log.AddLog("error deleting " + fileName + " " + e);
            }
        }

        public static void MoveFile(string oldPath, string newPath)
        {
            try
            {
                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);
                }
                Log.AddLog("moved " + oldPath + " > " + newPath);
            }
            catch (Exception e)
            {
                Log.AddLog("error moving " + oldPath + " > " + newPath + " " + e);
            }
        }

        public static void CleanupDirectory(string path, string extension)
        {
            int cleanBeforeDate;
            Int32.TryParse(DateTime.Now.AddDays(Const.HistoryToKeepInDays).ToString("yyyyMMdd"), out cleanBeforeDate);

            string[] files = GetFileNamesInDirectory(path, extension);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Length > 8)
                {
                    int fileDate;
                    Int32.TryParse(files[i].Substring(0, 8), out fileDate);
                    if (fileDate != 0 && fileDate < cleanBeforeDate)
                    {
                        DeleteFile(path, files[i]);
                    }
                }
            }
        }
    }
}

