using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class TimeSyncHelper
    {
        /// <summary>
        /// 从NTP服务器获取网络时间
        /// </summary>
        /// <param name="ntpIp"></param>
        /// <param name="webTime"></param>
        /// <returns></returns>
        public static bool GetWebTime(string ntpIp, ref DateTime webTime)
        {
            try
            {
                // default ntp server
                //const string ntpServer = "ntp1.aliyun.com";
                //const string ntpServer = "time.pool.google.com";

                IPAddress ipAddress;
                if (!IPAddress.TryParse(ntpIp, out ipAddress))
                {
                    IPAddress[] addresses = Dns.GetHostEntry(ntpIp).AddressList;
                    foreach (IPAddress address in addresses)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork) //只支持IPV4协议的IP地址
                        {
                            ipAddress = address;
                            break;
                        }
                    }
                }

                // NTP message size - 16 bytes of the digest (RFC 2030)
                byte[] ntpData = new byte[48];
                // Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                // The UDP port number assigned to NTP is 123
                //IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 123);

                // NTP uses UDP
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ipEndPoint);
                // Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();

                // Offset to get to the "Transmit Timestamp" field (time at which the reply 
                // departed the server for the client, in 64-bit timestamp format."
                const byte serverReplyTime = 40;
                // Get the seconds part
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                // Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
                // Convert From big-endian to little-endian
                intPart = SwapEndian(intPart);
                fractPart = SwapEndian(fractPart);
                ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000UL);

                // UTC time
                webTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(milliseconds);
                // Local time
                webTime = webTime.ToLocalTime();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("get WebTime infomation failed.", ex);
                return false;
            }
        }

        public static bool SetLocalTime(DateTime time)
        {
            NativeMethods.Systemtime sysTime = new NativeMethods.Systemtime();
            sysTime.FromDateTime(time);
            return NativeMethods.SetLocalTime(ref sysTime) == 0 ? true : false;
        }

        private static uint SwapEndian(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
            ((x & 0x0000ff00) << 8) +
            ((x & 0x00ff0000) >> 8) +
            ((x & 0xff000000) >> 24));
        }
    }
}
