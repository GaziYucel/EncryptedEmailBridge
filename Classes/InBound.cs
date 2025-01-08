using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.IO;

namespace EncryptedEmailBridge.Classes
{
    internal class InBound
    {
        public void Execute()
        {
            ImapClient imapClient = new ImapClient();
            imapClient.Connect(Const.EmailHost, Const.EmailPort, Const.EmailPortEncryption);
            imapClient.Authenticate(Const.EmailUsername, Const.EmailPassword);

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
                        fileName = Const.CurrentDateTimeStamp + "_" + attachmentCounter + "_" + attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        using (stream = File.Create(fileName))
                            rfc822.Message.WriteTo(stream);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        fileName = Const.CurrentDateTimeStamp + "_" + attachmentCounter + "_" + part.FileName;

                        using (stream = File.Create(fileName))
                            part.Content.DecodeTo(stream);
                    }
                    attachmentCounter++;
                }

                message.WriteTo(FormatOptions.Default, Const.RootPath + Const.ArchiveDirName + "\\" + Const.CurrentDateTimeStamp +
                    "_" + messageCounter + ".eml");

                inbox.Store(uid, new StoreFlagsRequest(StoreAction.Add, MessageFlags.Deleted));

                messageCounter++;
            }

            imapClient.Disconnect(true);
        }
    }
}

