using System;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ICPlatformTools;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ICPlatformTools
{
    public class SFTPUtil : IDisposable
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string RsaKeyPath { get; set; }

        private SftpClient client;

        private Task reconnectTask;

        public SFTPUtil(string host, int port, string userName, string password, string rsaKeyPath = null)
        {
            this.Host = host;
            this.Port = port;
            this.UserName = userName;
            this.Password = password;
            this.RsaKeyPath = rsaKeyPath;
            TimeOut = 1000;
        }

        public int TimeOut { get; set; }

        public bool Connect()
        {
            try
            {
                ConnectionInfo connectionInfo = null;
                if (!string.IsNullOrEmpty(RsaKeyPath))
                {
                    connectionInfo = new ConnectionInfo(Host, Port,
                                                UserName,
                        new PrivateKeyAuthenticationMethod(RsaKeyPath));
                }
                else
                {
                    connectionInfo = new ConnectionInfo(Host, Port,
                                                UserName,
                                                new PasswordAuthenticationMethod(UserName, Password));
                }
                client = new SftpClient(connectionInfo);
                client.Connect();
                LogHelper.Log.Debug("SFTP连接成功");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                return false;
            }
        }

        private async Task<bool> DirExistAsync(string remotePath)
        {
            return await Task.Factory.StartNew<bool>(() =>
            {
                try
                {
                    if (client != null && client.IsConnected)
                    {
                        client.ListDirectory(remotePath);
                        return true;
                    }
                    LogHelper.Log.Error("sftp服务未连接!");
                    Reconnect();
                    return false;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> MkdirAsync(string remotePath)
        {
            bool ret = await DirExistAsync(remotePath);
            if (!ret)
            {
                ret = await Task.Factory.StartNew<bool>(() =>
                {
                    try
                    {
                        if (client != null && client.IsConnected)
                        {
                            client.CreateDirectory(remotePath);
                            return true;
                        }
                        else
                        {
                            Reconnect();
                            return false;
                        }
                    }
                    catch (Exception ex) 
                    {
                        LogHelper.Log.Error(ex);
                        return false; 
                    }
                });
            }
            return ret;
        }


        public async Task<bool> UploadFileAsync(string fileName, string remotePath, bool isOverride)
        {
            bool ret = false;

            if (client != null && client.IsConnected)
            {
                ret = await Task.Factory.StartNew<bool>(() =>
                {
                    try
                    {
                        using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            client.UploadFile(fs, remotePath, isOverride);
                        }
                        return true;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        LogHelper.Log.Error("文件可能被占用跳过此文件等稍后重传", ex);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log.Error(ex.Message, ex);
                        return false;
                    }
                });
            }
            else
            {
                LogHelper.Log.Error("sftp服务未连接!");
                Reconnect();
                return false;
            }
            return ret;
        }

        private void Reconnect()
        {
            if (reconnectTask == null || reconnectTask.IsCompleted)
            {
                reconnectTask = Task.Run(() => {
                    Connect();
                });
            }
        }

        public void Dispose()
        {
            if (reconnectTask != null && !reconnectTask.IsCompleted)
            {
                Task.WaitAll(reconnectTask);
            }
        }
    }
}