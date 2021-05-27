using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace SFTPService
{
    public class SftpService : ISftpService
    {
        private readonly ILogger<SftpService> _logger;
        private readonly SftpConfig _config;

        byte[] expectedFingerPrint = new byte[] {
                                            0x66, 0x31, 0xaf, 0x00, 0x54, 0xb9, 0x87, 0x31,
                                            0xff, 0x58, 0x1c, 0x31, 0xb1, 0xa2, 0x4c, 0x6b
                                        };

        public SftpService(ILogger<SftpService> logger, SftpConfig sftpConfig)
        {
            _logger = logger;
            _config = sftpConfig;
        }

        public bool IsFileExists(string remoteFilename)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                return client.Exists(remoteFilename);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in listing files under [{remoteFilename}]");
                return false;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public IEnumerable<SftpFile> ListAllFiles(string remoteDirectory = ".")
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.HostKeyReceived += Client_HostKeyReceived;
                client.Connect();
                return client.ListDirectory(remoteDirectory);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void UploadFile(string localFilePath, string remoteFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                using var s = File.OpenRead(localFilePath);
                client.UploadFile(s, remoteFilePath);
                _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                client.DownloadFile(remoteFilePath, s);
                _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }

        public void DeleteFile(string remoteFilePath)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                client.DeleteFile(remoteFilePath);
                _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }

        public string[] Readfile(string remoteFilePath)
        {
            using var client = new SftpClient(
                                    _config.Host,
                                    _config.Port == 0 ? 22 : _config.Port,
                                    _config.UserName, 
                                    _config.Password);
            try
            {
                client.Connect();
                return client.ReadAllLines(remoteFilePath);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in reading file [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
            return new string[] {};
        }

        protected void Client_HostKeyReceived(object source, HostKeyEventArgs e)
        {
            if (expectedFingerPrint.Length == e.FingerPrint.Length)
            {
                for (var i = 0; i < expectedFingerPrint.Length; i++)
                {
                    if (expectedFingerPrint[i] != e.FingerPrint[i])
                    {
                        e.CanTrust = false;
                        break;
                    }
                }
            }
            else
            {
                e.CanTrust = false;
            }
        }

        public void CreateFile(string remoteFilePath, IEnumerable<string> contents)
        {
            using var client = new SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.HostKeyReceived += Client_HostKeyReceived;
                client.Connect();
                client.Create(remoteFilePath);
                Thread.Sleep(3000);
                client.WriteAllLines(remoteFilePath, contents);
                _logger.LogInformation($"File [{remoteFilePath}] created.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in creating file [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
        }
    }
}
