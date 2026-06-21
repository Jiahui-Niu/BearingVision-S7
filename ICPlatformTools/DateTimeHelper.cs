using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeHelper
    {
        public static readonly DateTime UnixTimeBase = new DateTime(1970, 1, 1).ToLocalTime();

        public static long ToTimestampMS(this DateTime dt)
        {
            return (long)dt.Subtract(UnixTimeBase).TotalMilliseconds;
        }

        public static DateTime TimestampToDateTime(long timestamp)
        {
            return UnixTimeBase.AddMilliseconds(timestamp);
        }

        public static string TimestampToDateTimeString(long timestamp)
        {
            // HH：24小时制 hh：12小时制
            var dt = TimestampToDateTime(timestamp);
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(UnixTimeBase).TotalMilliseconds;
        }
    }
}
