Encrypted Email Bridge
---------------------
Bridge between two or more locations on the Internet to send / receive files, using an email server / account.

Installation
---------------------
- copy the EncryptedEmailBridge folder to the desired location for example c:\EncryptedEmailBridge

Configuration
---------------------
- configure the settings in EncryptedEmailBridge.exe.config (App.config)
- in this file each setting is explained

Windows Task Scheduler
---------------------
- schedule a windows task for EncryptedEmailBridge.exe
- point the start folder to the EncryptedEmailBridge folder

Features Inbound
---------------------
- emails are retrieved via pop3
- all emails on the server are deleted
- attachments are saved
- if the attachment is an (encrypted) zip, it is first (decrypted), extracted and stored
- all attachments get a date timestamp prefix

Features Outbound
---------------------
- all files with certain extension in the folder are encrypted compressed and sent

Requirements
---------------------
- Microsoft Windows x64
- Microsoft .NET Framework 4.8

Dependencies
---------------------
- See [packages.config](packages.config)

Development
---------------------
- git clone https://github.com/GaziYucel/EncryptedEmailBridge
- open in Visual Studio Community 2022
- update / install dependent packackes with nuget

Contribute
---------------------
All help is welcome: asking questions, providing documentation, testing, or even development.

Please note that this project is released with a [Contributor Code of Conduct](CONDUCT.md). By participating in this project you agree to abide by its terms.

License
---------------------
This project is published under GNU General Public License, Version 3.
