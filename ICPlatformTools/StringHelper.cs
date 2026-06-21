using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class StringHelper
    {
        /// <summary>
        /// 将多个条码拼接成字符串
        /// </summary>
        /// <param name="list">条码列表</param>
        /// <returns></returns>
        public static string AppendString(ref List<string> list)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < list.Count(); i++)
                {
                    if (i != 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(list[i].Trim());
                }

                return builder.ToString();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("执行AppendString异常", ex);
                return string.Empty;
            }
        }

        /// <summary>        
        /// 时间戳转为C#格式时间        
        /// </summary>        
        /// <param name=”timeStamp”></param>        
        /// <returns></returns>        
        public static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        } 

        public static string utf8Decode(string u8str)
        {
            byte[] bytes = Encoding.Default.GetBytes(u8str);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
