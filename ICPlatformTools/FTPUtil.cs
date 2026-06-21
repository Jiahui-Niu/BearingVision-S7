using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;

namespace ICPlatformTools
{
    public class FTPUtil : IDisposable
    {
        private string _prefix = string.Empty;

        private FtpClient _ftpClient;

        public FTPUtil(string baseUrl, string userName, string password, bool usePassive = false, bool keepAlive = false)
        {
            var host = "127.0.0.1";
            var port = 21;

            if (!baseUrl.StartsWith("ftp://"))
            {
                host = "127.0.0.1";
                port = 21;
            }
			
            RetrieveHostPort(baseUrl, out host, out port, out _prefix);
            _ftpClient = new FtpClient(host, port, new System.Net.NetworkCredential(userName, password));
            if (usePassive)
            {
                _ftpClient.DataConnectionType = FtpDataConnectionType.PASV;
            }

            _ftpClient.SocketKeepAlive = keepAlive;

            try
            {
                _ftpClient.Connect();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }
        }

        ~FTPUtil()
        {
            if (!_ftpClient.IsDisposed)
            {
                _ftpClient.Dispose();
            }
        }

        public void Dispose()
        {
            if (!_ftpClient.IsDisposed)
            {
                _ftpClient.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public bool UploadFile(string fileFullName, string remoteFileName)
        {
            var newRemoteFileName = CombinePath(_prefix, remoteFileName);
            try
            {
                var ftpStatus = _ftpClient.UploadFile(fileFullName, newRemoteFileName, FtpRemoteExists.Skip, createRemoteDir: true);
                if (ftpStatus == FtpStatus.Success || ftpStatus == FtpStatus.Skipped)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                if (ex.InnerException != null)
                {
                    LogHelper.Log.Error(ex.InnerException.Message, ex.InnerException);
                }
            }

            return false;
        }

        private void RetrieveHostPort(string url, out string host, out int port, out string prefix)
        {
            // 默认 127.0.0.1:21
            if (url.StartsWith("ftp://"))
            {
                // 掐头去尾
                var trimedUrl = url.Replace("ftp://", "").TrimEnd('/');

                // 如果还带 '/' 号表示有prefix
                int firstIndexOfSlash = trimedUrl.IndexOf("/");
                if (firstIndexOfSlash > -1)
                {
                    var firstCommaIndex = trimedUrl.IndexOf(':');
                    if (firstCommaIndex > -1)
                    {
                        // 提取IP
                        host = trimedUrl.Substring(0, firstCommaIndex);

                        // 提取端口
                        var portStr = trimedUrl.Substring(firstCommaIndex + 1, firstIndexOfSlash - firstCommaIndex - 1);
                        if (!int.TryParse(portStr, out port))
                        {
                            port = 21;
                        }
                    }
                    else
                    {
                        host = trimedUrl.Substring(0, firstIndexOfSlash);
                        port = 21;
                    }

                    prefix = trimedUrl.Substring(firstIndexOfSlash, trimedUrl.Length - firstIndexOfSlash);

                    return;
                }
                else
                {
                    prefix = string.Empty;

                    var firstCommaIndex = trimedUrl.IndexOf(':');
                    if (firstCommaIndex > -1)
                    {
                        host = trimedUrl.Substring(0, firstCommaIndex);
                        var portStr = trimedUrl.Substring(firstCommaIndex + 1, trimedUrl.Length - firstCommaIndex - 1);
                        if (!int.TryParse(portStr, out port))
                        {
                            port = 21;
                        }
                    }
                    else
                    {
                        host = trimedUrl;
                        port = 21;
                    }

                    return;
                }
            }

            host = "127.0.0.1";
            port = 21;
            prefix = string.Empty;
        }

        // 合并prefix 和 上传路径
        private string CombinePath(string prefix, string path)
        {
            string ret = string.Empty;

            if (!prefix.StartsWith("/"))
            {
                ret = "/" + prefix;
            }

            if (prefix.EndsWith("/"))
            {
                if (path.StartsWith("/"))
                {
                    ret = prefix + path.TrimStart('/');
                }
                else
                {
                    ret = prefix + path;
                }
            }
            else
            {
                if (path.StartsWith("/"))
                {
                    ret = prefix + path;
                }
                else
                {
                    ret = prefix + "/" + path;
                }
            }

            return ret;
        }
    }
}
