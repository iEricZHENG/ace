using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;
using NLog.Config;

namespace App.Framework.Log
{
    public class LogManage
    {
        static string appName;
        static LogManage()
        {
            //NLog.LogManager.Configuration = new XmlLoggingConfiguration(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\NLog.config");
            NLog.LogManager.Configuration = new XmlLoggingConfiguration(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "NLog.config");
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
        public static string Exchange
        {
            get
            {
                return "SystemLog";
            }
        }
        public static ILogger GetLogger(string source, string appName = null)
        {
            if (string.IsNullOrEmpty(appName))
            {
                appName = AppName;
            }
            return new MQLogger(Exchange, appName, source, Level);
        }
        public static ILogger GetLogger(Type source, string appName = null)
        {
            if (string.IsNullOrEmpty(appName))
            {
                appName = AppName;
            }
            return new MQLogger(Exchange, appName, source.FullName, Level);
        }
    }
}
