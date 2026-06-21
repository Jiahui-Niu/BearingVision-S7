using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class HardwareInfoHelper
    {
        /// <summary>
        /// 获取CPUID
        /// </summary>
        /// <returns></returns>
        public static string GetCPUID()
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Processor");


                StringBuilder sb = new StringBuilder();

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sb.Append(queryObj["ProcessorId"]);
                }
                return sb.ToString();
            }
            catch (ManagementException e)
            {
                LogHelper.Log.Error("An error occurred while querying for WMI data: " + e.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }

            return "";
        }

        /// <summary>
        /// 获取主板序列号
        /// </summary>
        /// <returns></returns>
        public static string GetMotherBoardSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_BaseBoard");

                StringBuilder sb = new StringBuilder();

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    sb.Append(queryObj["SerialNumber"]);
                }
            }
            catch (ManagementException e)
            {
                LogHelper.Log.Error("An error occurred while querying for WMI data: " + e.Message);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error(ex.Message, ex);
            }
            return "";
        }

		/// <summary>
        /// 获取CPIID + 主板序列号
        /// </summary>
        /// <returns></returns>
        public static string GenerateDeviceID()
        {
            var cpuID = GetCPUID();
            var moID = GetMotherBoardSerialNumber();
            return HashHelper.GetMD5(System.Text.Encoding.UTF8.GetBytes(cpuID + moID));
        }
    }
}
