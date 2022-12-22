using System;

namespace App.Core
{
    public static class Extensions
    {
        public static DateTime ToLocalDateTime(this int target)
        {
            DateTime dtDateTime = new DateTime(621355968000000000 + (long)target * (long)10000000, DateTimeKind.Utc);
            return dtDateTime.ToLocalTime();
        }
        public static int ToUnixTimestamp(this DateTime target)
        {
            return (int)((target.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }
    }
}
