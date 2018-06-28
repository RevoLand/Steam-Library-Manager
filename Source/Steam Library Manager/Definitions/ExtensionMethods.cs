using System;
using System.IO;

namespace Steam_Library_Manager
{
    public static class ExtensionMethods
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }

        public static long ToUnixTimestamp(this DateTime d) => (long)(d - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

        public static bool IsDirectoryAccessible(this DirectoryInfo directory)
        {
            try
            {
                directory.GetAccessControl();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}