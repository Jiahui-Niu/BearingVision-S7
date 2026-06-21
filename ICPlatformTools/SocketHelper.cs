using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    

    public class SocketFactory
    {
        public enum SocketType
        {
            Server,
            Client,
        }

        /// <summary>
        /// 创建socket实例
        /// </summary>
        /// <param name="type"> socket类型 </param>
        /// <param name="ip"> ip </param>
        /// <param name="port"> 端口 </param>
        /// <param name="runHeart"> 是否开启心跳 </param>
        /// <returns></returns>
        public static ISocket Create(SocketType type, string ip, int port, bool runHeart = false)
        {
            switch (type)
            {
                case SocketType.Server:
                    return new SocketServerEx(ip, port);
                case SocketType.Client:
                    return new SocketClientEx(ip, port, runHeart);
                default:
                    return new SocketServerEx(ip, port);
            }
        }
    }

    public interface ISocket
    {
        Action<byte[]> RawReceiveHandler { get; set; }
        Action<string> ReceiveHandler { get; set; }
        bool IsConnect { get; }

        int Start();
        void Stop();
        int Send(byte[] data);
        int Send(string str);
    }

    class SocketServerEx : SocketServer, ISocket
    {
        public Action<byte[]> RawReceiveHandler
        {
            get { return base.RawMsgReceiveHandler; }
            set { base.RawMsgReceiveHandler = value; }
        }

        public SocketServerEx(string ip, int port)
            : base(ip, port)
        {

        }
    }

    class SocketClientEx : SocketClient, ISocket
    {
        public SocketClientEx(string ip, int port, bool runHeart = false)
            : base(ip, port, runHeart)
        {

        }

        public int Start()
        {
            return base.Connect();
        }

        public void Stop()
        {
            base.Dispose();
        }
    }
}