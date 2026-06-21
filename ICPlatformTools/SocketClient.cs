using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICPlatformTools;

namespace ICPlatformTools
{
    public class SocketClient : IDisposable
    {
        #region Variable and properties

        private byte[] buffer = new byte[4096];

        private Socket ClientSocket;

        private IPAddress ip;

        private int port;


        private Thread receiveThread;

        private Thread heartThread;

        private bool isRunning = false;

        private bool isRunHeart = false;

        private bool isFirstConnect = true;

        private Task reconnectTask;

        private bool isExiting = false;

        private DateTime lastHeartTime;

        private int recTimeOut = 1000;

        private List<byte> recBytes = new List<byte>();

        public Action<string> ReceiveHandler { get; set; }

        public Action<byte[]> RawReceiveHandler { get; set; }

        public Action<string> ErrorMessageHandler { get; set; }

        public Byte[] HeartContent { get; set; }

        public int HeartInterval { get; set; }

        public bool IsConnect 
        { 
            get 
            { 
                return isRunning;
            }
            set { isRunHeart = value; ConnectAction?.Invoke(value); }
        }

        public Action<bool> ConnectAction;
        #endregion

        /// <summary>
        /// 上一次发送数据的时间，控制下发送频率，防止粘包
        /// </summary>
        private DateTime lastSendDataTime = DateTime.Now;

        public SocketClient(string ip, int port, bool runHeart = false, int timeOut = 1000)
        {
            LogHelper.Log.Info("Socket连接成功");

            if (!IPAddress.TryParse(ip, out this.ip))
            {
                this.ip = IPAddress.Loopback;
            }
            this.port = port;
            isRunHeart = runHeart;
            HeartInterval = 1000;
            lastHeartTime = new DateTime(1970, 1, 1);
            recTimeOut = timeOut;
        }

        public int Connect()
        {
            try
            {
                if (ClientSocket == null)
                {
                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ClientSocket.SendTimeout = recTimeOut;
                }
                if (!ClientSocket.Connected)
                {
                    ClientSocket.Connect(ip, port);

                    if (ClientSocket.Connected)
                    {
                        LogHelper.Log.Info("Socket连接成功");
                        isRunning = true;
                        if (receiveThread == null)
                        {
                            receiveThread = new Thread(ReceiveTh) { IsBackground = true };
                            receiveThread.Start();
                        }
                        if (isRunHeart && heartThread == null)
                        {
                            heartThread = new Thread(HeartTh);
                            heartThread.Start();
                        }
                        return 0;
                    }
					else
					{
                        if (isFirstConnect)
                        {
                            isFirstConnect = false;
                            Reconect();
                            return 0;
                        }
					}
                }
            }
            catch (Exception ex)
            {
                if (ErrorMessageHandler != null)
                {
                    ErrorMessageHandler.Invoke(ex.Message);
                    LogHelper.Log.Error(ex.Message, ex);
                }
                if (isFirstConnect)
                {
                    isFirstConnect = false;
                    Reconect();
                }
            }
            Thread.Sleep(3000);
            return (int)ErrorCode.OpenTcpClientError;
        }

        public int Send(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Send(bytes);
        }

        public int Send(byte[] data)
        {
            try
            {

                // 每次发送前, 清空接收
                recBytes.Clear();

                if (ClientSocket != null && ClientSocket.Connected)
                {
                    int spendTime = (int)((DateTime.Now - lastSendDataTime).TotalMilliseconds);
                    if (spendTime <= 20)
                    {
                        Thread.Sleep(20 - spendTime);
                    }
                    LogHelper.Log.DebugFormat("[SocketClient] Send bytes: {0}", string.Join(",", BitConverter.ToString(data)));
                    lastSendDataTime = DateTime.Now;
                    if (ClientSocket != null && ClientSocket.Connected)
                    {
                        return (int)ClientSocket?.Send(data, 0, data.Length, SocketFlags.None);
                    }
                    else
                    {
                        Reconect();
                    }
                }
                else
                {
                    Reconect();
                }
            }
            catch (SocketException)
            {
                Reconect();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message,ex);
            }
            return 0;
        }
        
		// 同步发送带返回值
        public List<byte> SendAndCallback(byte[] data)
        {
            List<byte> sendData = new List<byte>();
            try
            {
                // 发送数据
                int ret = Send(data);
                if (ret > 0)
                {
                    // 超时时间内，接收数据
                    int sleepTime = recTimeOut / 10;
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(sleepTime);
                        if (recBytes.Count > 0)
                        {
                            sendData.AddRange(recBytes);
                            break;
                        }
                    }
                }
                return sendData;
            }
            catch(Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
                return sendData;
            }
        }


        private void ReceiveTh()
        {
            while (isRunning)
            {
                if (ClientSocket != null && ClientSocket.Connected)
                {
                    try
                    {
                        if (ClientSocket.Poll(1000, SelectMode.SelectRead) && (ClientSocket.Available == 0))
                        {
                            Reconect();
                            break;
                        }

                        if (ClientSocket.Available > 0)
                        {
                            int size = -1;
                            List<byte> byteBuilder = new List<byte>();
                            do
                            {
                                size = ClientSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                                byte[] data = new byte[size];
                                Array.Copy(buffer, data, size);
                                byteBuilder.AddRange(data);
                            }
                            while (ClientSocket.Available > 0);
                            if (ReceiveHandler != null)
                            {
                                ReceiveHandler.Invoke(Encoding.UTF8.GetString(byteBuilder.ToArray()));
                            }

                            if (RawReceiveHandler != null)
                            {
                                RawReceiveHandler.Invoke(byteBuilder.ToArray());
                            }
							// 接收数据
                            recBytes.AddRange(byteBuilder);

                            LogHelper.Log.DebugFormat("[SocketClient] receive bytes: {0}", string.Join(",", BitConverter.ToString(byteBuilder.ToArray())));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log.Error(ex.Message, ex);
                    }
                }
                else
                {
                    Reconect();
                }
                Thread.Sleep(10);
            }
        }

        private void HeartTh()
        {
            while (isRunning)
            {
                try
                {
                    if (DateTime.Now - lastHeartTime >= TimeSpan.FromMilliseconds(HeartInterval))
                    {
                        lastHeartTime = DateTime.Now;
                        if (ClientSocket != null && ClientSocket.Connected)
                        {
                            if (HeartContent != null && HeartContent.Length > 0)
                            {
                                Send(HeartContent);
                            }
                            else
                            {
                                Send(new byte[] { 0 });
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    Reconect();
                }
                catch (Exception)
                { }
                Thread.Sleep(50);
            }
        }

        public void DisConnect()
        {
            try
            {
                isRunning = false;
                FullClose(ClientSocket);
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join();
                }
                if (heartThread != null && heartThread.IsAlive)
                {
                    heartThread.Join();
                }
                receiveThread = null;
                heartThread = null;
                ClientSocket = null;
            }
            catch { }
        }

        private void Reconect()
        {
            if (reconnectTask == null || reconnectTask.IsCompleted)
            {
                reconnectTask = Task.Run(() =>
                {
                    while (!isExiting)
                    {
                        DisConnect();
                        if (Connect() == 0)
                        {
                            LogHelper.Log.Info("重连成功");
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                });
            }
        }

        private void FullClose(Socket socket)
        {
            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Disconnect(false);
                }
                try
                {
                    socket.Close();
                }
                catch (Exception) { }
                socket.Dispose();
            }  
        }

        public void Dispose()
        {
            isExiting = true;
            if (reconnectTask != null && !reconnectTask.IsCompleted)
            {
                reconnectTask.Wait();
            }
            DisConnect();
        }
    }
}
