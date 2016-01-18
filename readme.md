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
- Microsoft .NET Framework 4.5
- Chilkat 64-bit .NET 4.5 Assembly for VS2013
- Microsoft Visual C++ 2013 Redistributable x64
- Ionic's Zip Library 1.10.1.0

### Windows Task Scheduler

- schedule a windows task for EncryptedEmailBridge.exe
- point the start folder to the EncryptedEmailBridge folder
