using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    /// <summary>
    /// 校验工具
    /// </summary>
    public class CheckSumTools
    {
        /// <summary>
        /// BCC异或校验
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte BCCCheckSum(IEnumerable<byte> data)
        {
            if (data.Count() == 0)
            {
                return 0;
            }

            byte ret = data.First();
            for (int i = 1; i < data.Count(); i++)
            {
                ret ^= data.ElementAt(i);
            }

            return ret;
        }

        /// <summary>
        /// LRC校验
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte LRCCheckSum(IEnumerable<byte> data)
        {
            return (byte)(256 - (data.Sum(s => s) % 256));
        }
        
        
        //CRC校验 先高位再低位（左高右低）
        public static byte[] CRCCheckSumHiLo(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;
                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
                byte lo = (byte)(crc & 0x00FF);         //低位置
                return new byte[] { hi, lo };
            }
            else
                return new byte[] { 0, 0 };
        }

        //CRC校验 先低位再高位（左低右高）
        //顺丰龙门架
        public static byte[] CRCCheckSumLoHi(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;
                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
                byte lo = (byte)(crc & 0x00FF);         //低位置
                return new byte[] { lo, hi };
            }
            else
                return new byte[] { 0, 0 };
        }
    }
}
