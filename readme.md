# Encrypted Email Bridge

Bridge between two or more locations on the Internet to send / receive files, using an email server / account.

### Installation

- copy the EncryptedEmailBridge folder to the desired location for example c:\EncryptedEmailBridge

### Configuration

- configure the settings in EncryptedEmailBridge.exe.config (App.config)
- in this file each setting is explained

### Inbound

- emails are retrieved via pop3
- all emails on the server are deleted
- attachments are saved
- if the attachment is an (encrypted) zip, it is first (decrypted), extracted and stored
- all attachments get a date timestamp prefix

### Outbound

- all files with certain extension in the folder are encrypted compressed and sent

### Requirements

- Microsoft Windows x64
- Microsoft .NET Framework 4.8

### Dependencies

- DotNetZip 1.16.0
- MailKit 3.1.1
- MimeKit 3.1.1
- BouncyCastle.Crypto 1.9.0
- System.Buffers 4.5.1

### Development

- git clone https://github.com/GaziYucel/EncryptedEmailBridge
- open in Visual Studio Community 2022
- open pull request

### Windows Task Scheduler

- schedule a windows task for EncryptedEmailBridge.exe
- point the start folder to the EncryptedEmailBridge folder
