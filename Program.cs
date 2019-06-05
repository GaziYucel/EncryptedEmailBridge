using System;
using System.Configuration;
using System.IO;
using Ionic.Zip;
using MimeKit;
using MailKit;
using MailKit.Search;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;

namespace EncryptedEmailBridge
{
    class Program
    {
        private static string _method = string.Empty;
        private static string _path = string.Empty;
        private static string _extension = string.Empty;
        private static string _server = string.Empty;
        private static int _port;
        private static bool _portEncryption = false;
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
        private static string _workDir = "work";
        private static int _maxLengthFileName = 128;

        static void Main()
        {
            if (FillVariables() && CreateDirectories())
            {
                if (_method == "out") OutBound();
                if (_method == "in") InBound();

                // cleanup log files older than history
                if (_cleanupLog) CleanupDirectory(_path + _logDir + "\\", "txt");

                // cleanup archived files older than history
                if (_cleanupArchive) CleanupDirectory(_path + _archiveDir + "\\", _extension);
                if (_cleanupArchive) CleanupDirectory(_path + _archiveDir + "\\", "eml");
            }
            if (!WriteLog())
            {
                Console.WriteLine(_log);
            }
        }
        private static void InBound()
        {
            ImapClient imapClient = new ImapClient();
            imapClient.Connect(_server, _port, _portEncryption);
            imapClient.Authenticate(_username, _password);

            var inbox = imapClient.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var uids = inbox.Search(SearchQuery.All);

            int messageCounter = 0;
            foreach (var uid in uids)
            {
                var message = inbox.GetMessage(uid);
                Console.WriteLine("Subject: {0}", message.Subject);

                // do for each attachment
                var attachments = message.Attachments;
                int attachmentCounter = 0;
                foreach (var attachment in attachments)
                {
                    string fileName;
                    Stream stream;

                    if (attachment is MessagePart)
                    {
                        fileName = _dateTimeStamp + "_" + attachmentCounter + "_" + attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        using (stream = File.Create(fileName))
                            rfc822.Message.WriteTo(stream);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        fileName = _dateTimeStamp + "_" + attachmentCounter + "_" + part.FileName;

                        using (stream = File.Create(fileName))
                            part.Content.DecodeTo(stream);
                    }

                    attachmentCounter++;
                }

                message.WriteTo(FormatOptions.Default, _path + _archiveDir + "\\" + _dateTimeStamp + "_" + messageCounter + ".eml");

                inbox.Store(uid, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Deleted));

                messageCounter++;
            }

            imapClient.Disconnect(true);
        }
        private static void SaveAttachmentsItems(string pathWork, string pathOut, string attachment, int emailCounter, int attachmentCounter)
        {
            // attachment is a zip file; extract and save to out path
            if (attachment.Length > 3 && attachment.Substring(attachment.Length - 3, 3) == "zip")
            {
                // rename each file in zip
                try
                {
                    using (ZipFile zip = ZipFile.Read(pathWork + attachment))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            zip[i].FileName = ShortenFileName(_dateTimeStamp + "_" + emailCounter + attachmentCounter + i + "_" + zip[i].FileName);
                            AddLog("zip item renamed " + zip[i].FileName);
                        }
                        zip.Save();
                    }
                }
                catch (Exception e)
                {
                    AddLog("error renaming files in zip file " + attachment + " " + e);
                }

                // extract each file in zip
                try
                {
                    using (ZipFile zip = ZipFile.Read(pathWork + attachment))
                    {
                        for (int i = 0; i < zip.Count; i++)
                        {
                            if (zip[i].UsesEncryption)
                            {
                                zip[i].ExtractWithPassword(pathOut, ExtractExistingFileAction.OverwriteSilently, _secret);
                                AddLog("attachment (encrypted) " + zip[i].FileName + " saved");
                            }
                            else
                            {
                                zip[i].Extract(pathOut, ExtractExistingFileAction.OverwriteSilently);
                                AddLog("attachment (no-encryption) " + zip[i].FileName + " saved");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AddLog("error decompressing zip file " + attachment + " " + e);
                }

                DeleteFile(pathWork, attachment);
            }
            else // attachment non zip, save to out path
            {
                MoveFile(pathWork + attachment, pathOut + attachment);
                AddLog("attachment " + attachment + " saved");
            }
        }
        private static void OutBound()
        {
            // attachment name
            string attachment = _dateTimeStamp + "." + "zip";

            // get filenames in directory
            string[] files = GetFileNamesInDirectory(_path, _extension);

            // return if no files found
            if (files.Length == 0) { return; }

            // create zip file
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.Password = _secret;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    foreach (string file in files)
                    {
                        zip.AddFile(_path + file, "").FileName = ShortenFileName(file);
                    }
                    zip.Save(_path + attachment);
                }
            }
            catch (Exception e)
            {
                AddLog("error compressing files " + e);
            }

            // send mail if attachment found
            if (File.Exists(_path + attachment))
            {
                // send email with attachment and archive files if success
                if (SendEmailWithAttachment(_path, attachment))
                {
                    ArchiveFiles(_path, files);
                }

                // delete attachment file
                DeleteFile(_path, attachment);
            }
            else
            {
                AddLog("attachment nog found, email not sent: " + attachment);
            }
        }
        private static bool SendEmailWithAttachment(string path, string attachment)
        {
            bool local = true;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_sender, _sender));
            message.To.Add(new MailboxAddress(_recipient, _recipient));
            message.Subject = "Sent with " + _appName + " (attachment " + attachment + ")";
            message.Body = new TextPart("plain") { Text = "This message was automatically sent with " + _appName + "." };

            var builder = new BodyBuilder();
            builder.TextBody = "This message was automatically sent with " + _appName + ".";
            builder.Attachments.Add(path + attachment);
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(_server, _port, _portEncryption);
                client.Authenticate(_username, _password);
                client.Send(message);
                client.Disconnect(true);

                local = true;
            }

            return local;
        }

        private static string ShortenFileName(string file)
        {
            string local = file;

            if (local.Length > _maxLengthFileName)
            {
                int fileExtPos = local.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                if (fileExtPos >= 0)
                {
                    string name = local.Substring(0, fileExtPos);
                    string ext = local.Substring(fileExtPos + 1, local.Length - name.Length - 1);
                    name = name.Substring(0, _maxLengthFileName - ext.Length - 1).Trim();
                    local = name + "." + ext;
                }
                else
                {
                    local = local.Substring(0, _maxLengthFileName);
                }
                AddLog("file name shortened: " + file + " > " + local);
            }

            return local;
        }
        private static bool FillVariables()
        {
            try
            {
                _method = ConfigurationManager.AppSettings["method"];
                _path = ConfigurationManager.AppSettings["path"];
                _extension = ConfigurationManager.AppSettings["extension"];
                _server = ConfigurationManager.AppSettings["server"];
                Int32.TryParse(ConfigurationManager.AppSettings["port"], out _port);
                string portEncryption = ConfigurationManager.AppSettings["portEncryption"];
                if (portEncryption == "ssl" || portEncryption == "starttls") // ssl / starttls / plain
                {
                    _portEncryption = true;
                }
                else
                {
                    _portEncryption = false;
                }
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
                    !string.IsNullOrEmpty(portEncryption) &&
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
            return false;
        }
        private static bool CreateDirectories()
        {
            bool local = true;
            if (_path.Length > 3 && _path.Substring(_path.Length - 1, 1) != "\\")
            {
                _path += "\\";
            }
            string[] dirs = {
                _path,
                _path + _logDir,
                _path + _workDir,
                _path + _archiveDir };

            foreach (string dir in dirs)
            {
                try
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        AddLog("directory created: " + dir);
                    }
                }
                catch (Exception e)
                {
                    AddLog("create directory error: " + dir + " > " + e);
                    local = false;
                }
            }
            return local;
        }
        private static string[] GetFileNamesInDirectory(string path, string extension)
        {
            string[] files = Directory.GetFiles(path, "*." + extension, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace(path, "");
            }
            return files;
        }
        private static void ArchiveFiles(string path, string[] files)
        {
            foreach (string file in files)
            {
                try
                {
                    if (File.Exists(path + file))
                    {
                        File.Move(
                            path + file,
                            _path + _archiveDir + "\\" + ShortenFileName(_dateTimeStamp + "_" + file));

                    }
                    AddLog("archived " + file + " > " + ShortenFileName(_dateTimeStamp + "_" + file));
                }
                catch (Exception e)
                {
                    AddLog("error archiving " + file + ": " + e);
                }
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
        private static bool WriteLog()
        {
            try
            {
                using (StreamWriter w = File.AppendText(_path + _logDir + "\\" + _dateStamp + ".txt"))
                {
                    w.WriteLine("Date : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    if (!string.IsNullOrEmpty(_log))
                    {
                        w.WriteLine(_log);
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
        private static void DeleteFile(string filePath, string fileName)
        {
            try
            {
                if (File.Exists(filePath + fileName))
                {
                    File.Delete(filePath + fileName);
                    AddLog("deleted " + filePath + fileName);
                }
                else
                {
                    AddLog("not deleted (not found) " + filePath + fileName);
                }
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
