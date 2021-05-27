using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Renci.SshNet;
using SFTPService;

namespace SftpDemo
{
    internal class Program
    {
        private static void Main()
        {
            //var connectionInfo = new ConnectionInfo("sftp.foo.com",
            //                                        "guest",
            //                                        new PasswordAuthenticationMethod("guest", "pwd"),
            //                                        new PrivateKeyAuthenticationMethod("rsa.key"));

            //var config = new SftpConfig
            //{
            //    Host = "test.rebex.net",
            //    Port = 22,
            //    UserName = "demo",
            //    Password = "password"
            //};
            var sftpService = new SftpService(new NullLogger<SftpService>(), config);

            // list files
            var files = sftpService.ListAllFiles("/dcm/inbound");
            foreach (var file in files)
            {
                if (file.IsDirectory)
                {
                    Console.WriteLine($"Directory: [{file.FullName}]");
                }
                else if (file.IsRegularFile)
                {
                    Console.WriteLine($"File: [{file.FullName}]");
                }
            }

            // download a file
       /*     const string pngFile = @"hi.png";
            File.Delete(pngFile);
            sftpService.DownloadFile(@"/pub/example/imap-console-client.png", pngFile);
            if (File.Exists(pngFile))
            {
                Console.WriteLine($"file {pngFile} downloaded");
            }


            // upload a file // not working for this demo SFTP server due to readonly permission
            var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt");
            sftpService.UploadFile(testFile, @"/pub/test.txt");
            sftpService.DeleteFile(@"/pub/test.txt"); */


            // Create file at remote location
            const string csvFile = @"/dcm/inbound/SampleCsv.csv";
            var content = File.ReadAllLines(@"C:\Work\LifesLab\DCM Integration\Testing FTP Files\VSC-Orders-BC-20210512-111753AM_100.csv");
            sftpService.CreateFile(csvFile, content);
            if (sftpService.IsFileExists(csvFile))
            {
                Console.WriteLine($"file {csvFile} created at remote FTP location");
            }

            // Read file into memory from remote location
            const string csvRemoteFile = @"/dcm/inbound/SampleCsv.csv";
            var remoteContent = sftpService.Readfile(csvRemoteFile);
            if (remoteContent.Length > 0)
            {
                Console.WriteLine($"----------- file {csvRemoteFile} read from remote FTP location --------------- ");
                Console.WriteLine($"----------- file {csvRemoteFile} read from remote FTP location --------------- ");
            } 
            else
            {
                Console.WriteLine($"file {csvRemoteFile} read from remote FTP location - File is empty or not present");
            }
        }
    }
}
