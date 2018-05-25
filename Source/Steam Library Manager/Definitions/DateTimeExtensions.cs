using System;

namespace Steam_Library_Manager
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime d) => (long)(d - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }
}