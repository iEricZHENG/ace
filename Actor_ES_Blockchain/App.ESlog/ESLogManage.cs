using System;
using App.Framework.Log;
using App.ESLog;
using App.ESLog.ES;

namespace App.ESLog
{
    public class ESLogManage
    {
        static string appName;
        static ESLogManage()
        {
            appName = System.Configuration.ConfigurationManager.AppSettings["appName"];
            var levelStr = System.Configuration.ConfigurationManager.AppSettings["LogLevel"];
            if (!string.IsNullOrEmpty(levelStr))
            {
                level = (LogLevel)Enum.Parse(typeof(LogLevel), levelStr);
            }
        }
        static string AppName
        {
            get
            {
                return appName;
            }
        }
        static LogLevel level = LogLevel.Info;
        static LogLevel Level
        {
            get
            {
                return level;
            }
        }
        public static IESLogger GetLogger(string source, string appName = null)
        {
            if (string.IsNullOrEmpty(appName))
            {
                appName = AppName;
            }
            return new ESLogger(appName, source, Level);
        }
        public static IESLogger GetLogger(Type source, string appName = null)
        {
            if (string.IsNullOrEmpty(appName))
            {
                appName = AppName;
            }
            return new ESLogger(appName, source.FullName, Level);
        }
    }
}
