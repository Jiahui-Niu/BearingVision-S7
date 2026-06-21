using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ICPlatformTools
{
    public class SocketServer : IDisposable
    {
        private Socket _serverSocket;
        private readonly List<Socket> _clients = new List<Socket>();
        private readonly object _clientLock = new object();
        private Thread _acceptThread;
        private bool _isRunning;
        private readonly int _port;
        private readonly byte[] _buffer = new byte[4096];

        public Action<string> ReceiveHandler { get; set; }       // (message)
        public Action<byte[]> RawMsgReceiveHandler { get; set; } // (raw data)
        public Action<string, bool> ClientConnectAction { get; set; } // (ip, connected)

        public bool IsConnect => _isRunning;
        public bool IsRunning => _isRunning;

        public SocketServer(string ip, int port)
        {
            _port = port;
        }

        public int Start()
        {
            try
            {
                if (_isRunning) return 0;
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
                _serverSocket.Listen(10);
                _isRunning = true;
                _acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "SocketServer_Accept" };
                _acceptThread.Start();
                LogHelper.Log.Info($"SocketServer 已启动，监听端口 {_port}");
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("SocketServer 启动失败", ex);
                return -1;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try { _serverSocket?.Close(); } catch { }
            lock (_clientLock)
            {
                foreach (var c in _clients) { try { c.Close(); } catch { } }
                _clients.Clear();
            }
            LogHelper.Log.Info("SocketServer 已停止");
        }

        public int Send(byte[] data)
        {
            lock (_clientLock)
            {
                var dead = new List<Socket>();
                foreach (var c in _clients)
                {
                    try { c.Send(data); }
                    catch { dead.Add(c); }
                }
                foreach (var c in dead) _clients.Remove(c);
            }
            return 0;
        }

        public int Send(string message) => Send(Encoding.UTF8.GetBytes(message));

        public void SendAll(byte[] data) => Send(data);
        public void SendAll(string message) => Send(message);

        private void AcceptLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var client = _serverSocket.Accept();
                    var ip = (client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
                    LogHelper.Log.Info($"SocketServer 新客户端连接: {ip}");
                    lock (_clientLock) { _clients.Add(client); }
                    ClientConnectAction?.Invoke(ip, true);
                    var t = new Thread(() => ReceiveLoop(client, ip)) { IsBackground = true };
                    t.Start();
                }
                catch (Exception ex)
                {
                    if (_isRunning) LogHelper.Log.Error("SocketServer Accept 异常", ex);
                }
            }
        }

        private void ReceiveLoop(Socket client, string ip)
        {
            while (_isRunning && client.Connected)
            {
                try
                {
                    if (client.Poll(1000, SelectMode.SelectRead) && client.Available == 0)
                        break;

                    if (client.Available > 0)
                    {
                        var buf = new List<byte>();
                        do
                        {
                            int n = client.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);
                            var chunk = new byte[n];
                            Array.Copy(_buffer, chunk, n);
                            buf.AddRange(chunk);
                        } while (client.Available > 0);

                        var bytes = buf.ToArray();
                        RawMsgReceiveHandler?.Invoke(bytes);
                        ReceiveHandler?.Invoke(Encoding.UTF8.GetString(bytes));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error($"SocketServer 接收异常 [{ip}]", ex);
                    break;
                }
                Thread.Sleep(10);
            }

            lock (_clientLock) { _clients.Remove(client); }
            try { client.Close(); } catch { }
            ClientConnectAction?.Invoke(ip, false);
            LogHelper.Log.Info($"SocketServer 客户端断开: {ip}");
        }

        public void Dispose() => Stop();
    }
}
