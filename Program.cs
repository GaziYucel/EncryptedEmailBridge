using System;
using System.Configuration;
using System.IO;
using Chilkat;
using Ionic.Zip;
using Ionic.Zlib;

namespace EncryptedEmailBridge
{
    class Program
    {
        private static string _method = string.Empty;
        private static string _path = string.Empty;
        private static string _extension = string.Empty;
        private static string _server = string.Empty;
        private static int _port;
        private static string _sender = string.Empty;
        private static string _recipient = string.Empty;
        private static string _username = string.Empty;
        private static string _password = string.Empty;
        private static string _secret = string.Empty;
        private static int _history;
        private static bool _cleanupLog;
        private static bool _cleanupArchive;

        private static string _appName = "EncryptedEmailBridge";
        private static string _dateStamp = DateTime.Now.ToString("yyyyMMdd");
        private static string _timeStamp = DateTime.Now.ToString("HHmmss_ffff");
        private static string _dateTimeStamp = _dateStamp + "_" + _timeStamp;
        private static string _log = string.Empty;
        private static string _logDir = "log";
        private static string _archiveDir = "archive";
        private static string _appDir = Environment.CurrentDirectory + "\\";
        private static string _workDir = "work";
        private static string _chilkatLicense = "ADD-VALID-LICENSE-KEY";

        static void Main()
        {
            if (FillVariables() && CreateDirectories())
            {
                if (_method == "out")
                {
                    OutBound();
                }

                if (_method == "in")
                {
                    InBound();
                }

                // cleanup log files older than history
                if (_cleanupLog)
                {
                    CleanupDirectory(_appDir + _logDir + "\\", "txt");
                }

                // cleanup archived files older than history
                if (_cleanupArchive)
                {
                    CleanupDirectory(_path + _archiveDir + "\\", _extension);
                    CleanupDirectory(_path + _archiveDir + "\\", "eml");
                }
                
                // write to logfile
                try
                {
                    using (StreamWriter w = File.AppendText(_appDir + _logDir + "\\" + _dateStamp + ".txt"))
                    {
                        w.WriteLine("Datum : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        w.WriteLine(_log);
                        w.WriteLine("---------------------------");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("fout schrijven log: " + e);
                }
            }
        }
        private static void InBound()
        {
            // get emails from server and save attachment(s) and eml(s)
            MailMan mailman = new MailMan();
            mailman.UnlockComponent(_chilkatLicense);
            mailman.AutoFix = true;
            mailman.MailHost = _server;
            mailman.MailPort = _port;
            mailman.PopUsername = _username;
            mailman.PopPassword = _password;
            EmailBundle bundle = null;

            // get emails and copy to bundle object
            try { bundle = mailman.CopyMail(); } catch (Exception e) { AddLog("error getting emails " + e);  }
            if (bundle == null) { return; }

            // do for each email
            for (int i = 0; i <= bundle.MessageCount - 1; i++)
            {
                Email email = bundle.GetEmail(i);
                // do for each attachment
                for (int j = 0; j < email.NumAttachments; j++)
                {
                    string attachment = DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + "_" + email.GetAttachmentFilename(j);
                    email.SetAttachmentFilename(j, attachment);

                    // save attachment to filesystem
                    try { email.SaveAttachedFile(j, _appDir + _workDir); } catch (Exception e) { AddLog("error saving attachment  " + attachment + " " + e); }

                    if (attachment.Length > 3 && attachment.Substring(attachment.Length - 3, 3) == "zip")
                    {
                        // rename each file in zip
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(_appDir + _workDir + "\\" + attachment))
                            {
                                for (int k = 0; k < zip.Count; k++)
                                {
                                    zip[k].FileName = _dateTimeStamp + "_" + zip[k].FileName;
                                    AddLog("zip item renamed " + zip[k].FileName);
                                }
                                zip.Save();
                            }
                        } catch (Exception e) { AddLog("error renaming files in zip file " + attachment + " " + e); }

                        // extract each file in zip
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(_appDir + _workDir + "\\" + attachment))
                            {
                                for (int k = 0; k < zip.Count; k++)
                                {
                                    if (zip[k].UsesEncryption)
                                    {
                                        zip[k].ExtractWithPassword(_path, ExtractExistingFileAction.OverwriteSilently, _secret);
                                        AddLog("attachment (encrypted) " + zip[k].FileName + " saved");
                                    }
                                    else
                                    {
                                        zip[k].Extract(_path, ExtractExistingFileAction.OverwriteSilently);
                                        AddLog("attachment (no-encryption) " + zip[k].FileName + " saved");
                                    }
                                }
                            }
                        } catch (Exception e) { AddLog("error decompressing zip file " + attachment + " " + e); }

                        DeleteFile(_appDir + _workDir + "\\", attachment);
                    }
                    else
                    {
                        MoveFile(_appDir + _workDir + "\\" + attachment, _path + attachment);
                        AddLog("attachment " + attachment + " saved");
                    }
                }

                // archive email message as eml
                try
                {
                    email.SaveEml(_path + _archiveDir + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + ".eml");
                    AddLog("email " + DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + ".eml" + " saved");
                } catch (Exception e) { AddLog("error saving eml " + e); }
            }
            mailman.DeleteBundle(bundle);
            mailman.Pop3EndSession();
        }
        private static void OutBound()
        {
            string attachment = string.Empty;
            string[] files = Directory.GetFiles(_path, "*." + _extension, SearchOption.TopDirectoryOnly);

            // return if no files found
            if (files.Length == 0) { return; }

            // get only the filenames
            for (int i = 0; i < files.Length; i++) { files[i] = files[i].Replace(_path, ""); }

            // construct attachment name
            for (int i = 0; i < files.Length; i++) { attachment += files[i].Replace(" ", "_").Replace(".", "_") + "_"; }
            attachment = attachment.Replace("__", "_");
            attachment = attachment.Substring(0, attachment.Length - 1);
            if (attachment.Length > 248) { attachment = attachment.Substring(0, 248); }
            attachment +=  ".zip";

            // create zip file
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.Password = _secret;
                    zip.CompressionLevel = CompressionLevel.BestCompression;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    for (int i = 0; i < files.Length; i++) { zip.AddFile(_path + files[i], ""); }
                    zip.Save(_path + attachment);
                }
            } catch (Exception e) { AddLog("error compressing files " + e); }

            // send mail
            try
            {
                MailMan mailman = new MailMan();
                mailman.UnlockComponent(_chilkatLicense);
                mailman.AutoFix = true;
                mailman.SmtpHost = _server;
                mailman.SmtpPort = _port;
                mailman.SmtpUsername = _username;
                mailman.SmtpPassword = _password;
                Email email = new Email();
                email.From = _sender;
                email.AddTo(_recipient, _recipient);
                email.Subject = "Verzonden met " + _appName;
                email.Body = attachment;
                email.AddFileAttachment(_path + attachment);
                if (mailman.SendEmail(email)) { AddLog("email sent with attachment " + attachment); } else { AddLog("error sending mail  " + mailman.LastErrorText); }
                mailman.CloseSmtpConnection();
            } catch (Exception e) { AddLog("error sending mail " + e); }

            // archive files
            for (int i = 0; i < files.Length; i++) { ArchiveFile(_path, files[i]); }

            // delete attachment file
            DeleteFile(_path, attachment);
        }

        private static bool FillVariables()
        {
            if (File.Exists(_appName + ".exe.config"))
            {
                try
                {
                    _method = ConfigurationManager.AppSettings["method"];
                    _path = ConfigurationManager.AppSettings["path"];
                    _extension = ConfigurationManager.AppSettings["extension"];
                    _server = ConfigurationManager.AppSettings["server"];
                    Int32.TryParse(ConfigurationManager.AppSettings["port"], out _port);
                    _sender = ConfigurationManager.AppSettings["sender"];
                    _recipient = ConfigurationManager.AppSettings["recipient"];
                    _username = ConfigurationManager.AppSettings["username"];
                    _password = ConfigurationManager.AppSettings["password"];
                    _secret = ConfigurationManager.AppSettings["secret"];
                    Int32.TryParse(ConfigurationManager.AppSettings["history"], out _history);
                    _cleanupLog = Convert.ToBoolean(ConfigurationManager.AppSettings["cleanupLog"]);
                    _cleanupArchive = Convert.ToBoolean(ConfigurationManager.AppSettings["cleanupArchive"]);

                    if (
                        !string.IsNullOrEmpty(_method) &&
                        !string.IsNullOrEmpty(_path) && 
                        !string.IsNullOrEmpty(_extension) && 
                        !string.IsNullOrEmpty(_server) &&
                        _port != 0 && 
                        !string.IsNullOrEmpty(_sender) &&
                        !string.IsNullOrEmpty(_recipient) &&
                        !string.IsNullOrEmpty(_username) &&
                        !string.IsNullOrEmpty(_password) &&
                        !string.IsNullOrEmpty(_secret) &&
                        _history != 0
                    )
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    AddLog("configuration: error > " + e);
                }
            }
            return false;
        }
        private static bool CreateDirectories()
        {
            try
            {
                if (_path.Length > 3 && _path.Substring(_path.Length - 1, 1) != "\\") { _path += "\\"; }
                if (!Directory.Exists(_path)) { Directory.CreateDirectory(_path); }
                if (!Directory.Exists(_path + _archiveDir)) { Directory.CreateDirectory(_path + _archiveDir); }
                if (!Directory.Exists(_appDir + _logDir)) { Directory.CreateDirectory(_appDir + _logDir); }
                if (!Directory.Exists(_appDir + _workDir)) { Directory.CreateDirectory(_appDir + _workDir); }
                return true;
            }
            catch (Exception e)
            {
                AddLog("create directories: error > " + e);
                return false;
            }
        }
        private static void ArchiveFile(string filePath, string fileName)
        {
            try
            {
                if (File.Exists(filePath + fileName))
                {
                    File.Move(filePath + fileName, _path + _archiveDir + "\\" + _dateTimeStamp + "_" + fileName);
                }
                AddLog("archived " + fileName + " > " + _dateTimeStamp + "_" + fileName);
            }
            catch (Exception e)
            {
                AddLog("error archiving " + fileName + ": " + e);
            }
        }
        private static void AddLog(string logMessage)
        {
            logMessage = logMessage.Replace(Environment.NewLine, " ");

            if (!string.IsNullOrEmpty(_log))
            {
                _log += Environment.NewLine;
            }

            _log += logMessage;
        }
        private static void DeleteFile(string filePath, string fileName)
        {
            try
            {
                if (File.Exists(filePath + fileName))
                {
                    File.Delete(filePath + fileName);
                }
                AddLog("deleted " + fileName);
            }
            catch (Exception e)
            {
                AddLog("error deleting " + fileName + " " + e);
            }
        }
        private static void MoveFile(string oldPath, string newPath)
        {
            try
            {
                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);
                }
                AddLog("moved " + oldPath + " > " + newPath);
            }
            catch (Exception e)
            {
                AddLog("error moving " + oldPath + " > " + newPath + " " + e);
            }
        }
        private static void CleanupDirectory(string path, string extension)
        {
            int cleanBeforeDate;
            Int32.TryParse(DateTime.Now.AddDays(-_history).ToString("yyyyMMdd"), out cleanBeforeDate);

            string[] files = Directory.GetFiles(path, "*." + extension, SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(path, "");
                bool deleteFile = false;
                if (files[i].Length > 8)
                {
                    int fileDate;
                    Int32.TryParse(files[i].Substring(0, 8), out fileDate);
                    if (fileDate != 0 && fileDate < cleanBeforeDate)
                    {
                        deleteFile = true;
                    }
                }
                else
                {
                    deleteFile = true;
                }

                if (deleteFile)
                {
                    try
                    {
                        DeleteFile(path + "\\", files[i]);
                        AddLog("archive file " + files[i] + " > deleted");
                    }
                    catch (Exception e)
                    {
                        AddLog("error deleting log file " + files[i] + ": " + e);
                    }
                }

            }
        }
    }
}
