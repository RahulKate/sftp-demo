using System.Collections.Generic;
using Renci.SshNet.Sftp;

namespace SFTPService
{
    public interface ISftpService
    {
        IEnumerable<SftpFile> ListAllFiles(string remoteDirectory = ".");
        void UploadFile(string localFilePath, string remoteFilePath);
        void DownloadFile(string remoteFilePath, string localFilePath);
        void DeleteFile(string remoteFilePath);
        string[] Readfile(string remoteFilePath);
        public void CreateFile(string remoteFilePath, IEnumerable<string> contents);
    }
}
