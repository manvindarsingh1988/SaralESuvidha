using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPCLLibrary
{

    public static class UnixTimestampConverter
    {
        // Convert DateTime to Unix Timestamp (Milliseconds)
        public static long ToUnixTimestampMillis(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
        }

        // Convert Unix Timestamp (Milliseconds) to DateTime
        public static DateTime FromUnixTimestampMillis(long unixTimestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp).UtcDateTime;
        }


        // Convert DateTime to Unix Timestamp (Milliseconds) with GMT+5:30 Offset
        public static long ToUnixTimestampMillisIST(DateTime dateTime)
        {
            TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTimeOffset istDateTime = TimeZoneInfo.ConvertTime(dateTime, istTimeZone);
            return istDateTime.ToUnixTimeMilliseconds();
        }

        // Convert Unix Timestamp (Milliseconds) to DateTime (IST)
        public static DateTime FromUnixTimestampMillisIST(long unixTimestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
            TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            return TimeZoneInfo.ConvertTime(dateTimeOffset.UtcDateTime, istTimeZone);
        }


    }
}
