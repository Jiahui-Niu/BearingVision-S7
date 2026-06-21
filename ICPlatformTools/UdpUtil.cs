using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ICPlatformTools
{
    public class UdpUtil
    {
        private Socket socket;

        private Thread receiveThread;

        private Thread heartThread;

        private bool isRunning = false;

        private bool isRunHeart = false;

        private EndPoint localEndPoint;

        private EndPoint remoteEndPoint;

        private byte[] buffer = new byte[4096];

        private DateTime lastHeartTime;

        public Action<string> ReceiveHandler { get; set; }

        public Action<byte[]> RawReceiveHandler { get; set; }

        public Action<string> ErrorMessageHandler { get; set; }

        public int HeartInterval { get; set; }

        public byte[] HeartContent { get; set; }

        public UdpUtil(string localip, int localport, string remoteip, int remoteport, bool runHeart = false)
        {
            localEndPoint = new IPEndPoint(IPAddress.Parse(localip), localport);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteip), remoteport);
            isRunHeart = runHeart;
            HeartInterval = 2000;       // default value
        }

        public int Start()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            try
            {
                socket.Bind(localEndPoint);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                return (int)ErrorCode.SocketBindPortError;
            }
            isRunning = true;
            receiveThread = new Thread(ReceiveMsg) { IsBackground = true };
            receiveThread.Start();

            if (isRunHeart)
            {
                heartThread = new Thread(SendingHeart) { IsBackground = true };
                heartThread.Start();
            }
            return 0;
        }

        public void Stop()
        {
            isRunning = false;
            if (socket != null)
            {
                socket.Close();
                socket.Dispose();
                socket = null;
            }
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join();
            }

            if (heartThread != null && heartThread.IsAlive)
            {
                heartThread.Join();
            }
        }

        public bool Send(string str)
        {
            return SendTo(str, remoteEndPoint);
        }

        public bool Send(byte[] bytes)
        {
            return SendTo(bytes, remoteEndPoint);
        }

        public bool SendTo(string str, EndPoint endpoint)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return SendTo(bytes, remoteEndPoint);
        }

        public bool SendTo(byte[] bytes, EndPoint endPoint)
        {
            try
            {
                if (socket != null)
                {
                    if (socket.SendTo(bytes, endPoint) > 0)
                    {
                        LogHelper.Log.DebugFormat("[UdpUtil] Send bytes: {0}", BitConverter.ToString(bytes));
                        return true;
                    }
                }

                LogHelper.Log.ErrorFormat("[UdpUtil] Send bytes: {0}", BitConverter.ToString(bytes));
                return false;

            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("error, try to send message through udp", ex);
                return false;
            }
        }

        private void ReceiveMsg()
        {
            while (isRunning) 
            {
                try
                {
                    if (socket.Available > 0)
                    {
                        int size = -1;
                        var byteBuilder = new List<byte>();
                        do
                        {
                            size = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                            var data = new byte[size];
                            Array.Copy(buffer, data, size);
                            byteBuilder.AddRange(data);
                        }
                        while (socket.Available > 0);

                        if (ReceiveHandler != null)
                        {
                            ReceiveHandler.Invoke(Encoding.UTF8.GetString(byteBuilder.ToArray()));
                        }
                        if (RawReceiveHandler != null)
                        {
                            RawReceiveHandler.Invoke(byteBuilder.ToArray());
                        }
                        LogHelper.Log.DebugFormat("[UdpUtil] receive bytes: {0}", BitConverter.ToString(byteBuilder.ToArray()));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error(ex.Message, ex);
                }
                Thread.Sleep(20);
            }
        }

        private void SendingHeart()
        {
            while (isRunning)
            {
                if (DateTime.Now - lastHeartTime > TimeSpan.FromMilliseconds(HeartInterval))
                {
                    if (HeartContent == null || HeartContent.Length == 0)
                        Send(new byte[] { 0 });
                    else
                        Send(HeartContent);

                    lastHeartTime = DateTime.Now;
                }

                Thread.Sleep(100);
            }
        }
    }
}
