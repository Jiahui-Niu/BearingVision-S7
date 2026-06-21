using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICPlatformTools
{
    public class LogModel
    {
        public string LogLevel { get; set; }

        public string LogType { get; set; }

        public string LogContent { get; set; }

        public string LogTime { get; set; }
    }

    public class Log2
    {
        public static void LogError(object sender, string fmt, params object[] args)
        {
            WriteLog(sender, "ERROR", fmt, args);
        }
        public static void LogWarn(object sender, string fmt, params object[] args)
        {
            WriteLog(sender, "WARNING", fmt, args);
        }

        public static void LogInfo(object sender, string fmt, params object[] args)
        {
            WriteLog(sender, "INFO", fmt, args);
        }

        public static void LogDebug(object sender, string fmt, params object[] args)
        {
            WriteLog(sender, "DEBUG", fmt, args);
        }

        public static void WriteLog(object sender, string level, string format, params object[] args)
        {
            try
            {
                var msg = string.Format(format, args);
                EventCenter.Instance.Notify(EventCenter.EventID.Logger, new EventData(sender, new LogModel()
                {
                    LogLevel = level,
                    LogType = sender.GetType().Name,
                    LogContent = msg,
                    LogTime = DateTime.Now.ToUnixTimeStamp().ToString()
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("Write db log failed.", ex);
            }
        }

        /*
         * 打印PLCNG时间, 即停称时刻
         * */
        public static void LogPLCTime(long codeTimeStamp, bool isNg = true)
        {
            var now = DateTime.Now.ToUnixTimeStamp();
            if (isNg)
            {
                LogHelper.Log.InfoFormat("[PLC::Tag] isNg:[true] Tplcng:[{0}], Tup:[{1}], Tplcng-Tup:[{2}].", now, codeTimeStamp, now - codeTimeStamp);
            }
            else
            {
                LogHelper.Log.InfoFormat("[PLC::Tag] isNg:[false] Tplcok:[{0}], Tup:[{1}], Tplcok-Tup:[{2}].", now, codeTimeStamp, now - codeTimeStamp);
            }
        }

        /*
         * 打印WCS时间, 即发送分拣信息时刻, 若无分拣信息则用条码发送时刻
         * */
        public static void LogWCSTime(long codeTimeStamp)
        {
            var now = DateTime.Now.ToUnixTimeStamp();
            LogHelper.Log.InfoFormat("[WCS::Tag] Twcs:[{0}], Tup:[{1}], Twcs-Tup:[{2}].", now, codeTimeStamp, now - codeTimeStamp);
        }
    }
}
