using Ionic.Zip;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.IO;

namespace EncryptedEmailBridge.Classes
{
    internal class OutBound
    {
        public void Execute()
        {
            // attachment name
            string attachment = Const.CurrentDateTimeStamp + "." + "zip";

            // get filenames in directory
            string[] files = Util.GetFileNamesInDirectory(Const.RootPath, Const.Extension);

            // return if no files found
            if (files.Length == 0) { return; }

            // create zip file
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.Password = Const.EncryptionSecret;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                    foreach (string file in files)
                    {
                        zip.AddFile(Const.RootPath + file, "").FileName = Util.ShortenFileName(file);
                    }
                    zip.Save(Const.RootPath + attachment);
                }
            }
            catch (Exception e)
            {
                Log.AddLog("error compressing files " + e);
            }

            // send mail if attachment found
            if (File.Exists(Const.RootPath + attachment))
            {
                // send email with attachment and archive files if success
                if (SendEmailWithAttachment(Const.RootPath, attachment))
                {
                    Util.ArchiveFiles(Const.RootPath, files);
                }

                // delete attachment file
                Util.DeleteFile(Const.RootPath, attachment);
            }
            else
            {
                Log.AddLog("attachment nog found, email not sent: " + attachment);
            }
        }

        public bool SendEmailWithAttachment(string path, string attachment)
        {
            bool local = true;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Const.EmailSender, Const.EmailSender));
            message.To.Add(new MailboxAddress(Const.EmailRecipient, Const.EmailRecipient));
            message.Subject = "Sent with " + Const.AppName + " (attachment " + attachment + ")";
            message.Body = new TextPart("plain") { Text = "This message was automatically sent with " + Const.AppName + "." };

            var builder = new BodyBuilder();
            builder.TextBody = "This message was automatically sent with " + Const.AppName + ".";
            builder.Attachments.Add(path + attachment);
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(Const.EmailHost, Const.EmailPort, Const.EmailPortEncryption);
                client.Authenticate(Const.EmailUsername, Const.EmailPassword);
                client.Send(message);
                client.Disconnect(true);

                local = true;
            }

            return local;
        }
    }
}
