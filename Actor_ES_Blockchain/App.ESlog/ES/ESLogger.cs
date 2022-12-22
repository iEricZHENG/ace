using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using App.Framework.Log;

namespace App.ESLog.ES
{
    public class ESLogger : IESLogger
    {
        static NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger(typeof(ESLogger));
        static ESLogger()
        {
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    ipAddress = _IPAddress.ToString();
                }
            }
        }
        static string ipAddress;
        string appName, source;
        LogLevel level;
        public ESLogger(string appName, string source, LogLevel level)
        {
            this.appName = appName;
            this.source = source;
            this.level = level;
        }
        private static async Task Insert(LogInfo data, string index)
        {
            var response = await ESClient.Client.IndexAsync(data, idx => idx.Index(index.ToLower()));
            //需要做异常判断处理
            if (!response.IsValid)
            {
                logger.Fatal(response.OriginalException, JsonConvert.SerializeObject(data));
            }
        }
        private static async Task Insert<T>(EventLog<T> data, string index)
        {
            var response = await ESClient.Client.IndexAsync(data, idx => idx.Index(index.ToLower()));
            //需要做异常判断处理
            if (!response.IsValid)
            {
                logger.Fatal(response.OriginalException, JsonConvert.SerializeObject(data));
            }
        }
        #region debug
        public async Task Debug(string message)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Debug<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }
        #endregion
        #region Error
        public async Task Error(string message)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Error(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Error(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Error<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Error<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Error<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Error;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }
        #endregion
        public async Task Event<T>(string message, T data)
        {
            EventLog<T> info = new EventLog<T>();
            info.Message = message;
            info.IPAddress = ipAddress;
            info.EventData = data;
            info.Source = source;
            info.SourceApp = appName;
            info.Time = DateTime.UtcNow;
            var index = typeof(T).FullName;
            await Insert<T>(info, index);
        }
        #region Fatal
        public async Task Fatal(string message)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Fatal(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Fatal(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Fatal<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Fatal<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Fatal<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Fatal;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }
        #endregion
        #region Info
        public async Task Info(string message)
        {

            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Info(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Info(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Info<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Info<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Info<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Info;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }
        #endregion
        public async Task Log(LogInfo info, object data = null)
        {
            if (info.Level >= level)
            {
                try
                {
                    info.Source = source;
                    info.SourceApp = appName;
                    info.Time = DateTime.UtcNow;
                    var index = appName + "_SystemLogs";
                    if (data != null)
                    {
                        info.JsonData = JsonConvert.SerializeObject(data);
                    }
                    await Insert(info, index);
                }
                catch (Exception e)
                {
                    logger.Fatal(e);
                }
            }
        }
        #region Trace
        public async Task Trace(string message)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Trace(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Trace(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Trace<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Trace<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Trace<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Trace;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }
        #endregion
        #region Warning
        public async Task Warning(string message)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Warning(Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Warning(string message, Exception e)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Warning<T>(string message, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Warning<T>(Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log, data);
        }

        public async Task Warning<T>(string message, Exception e, T data)
        {
            LogInfo log = new LogInfo();
            log.Level = LogLevel.Warning;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }
        #endregion
    }
}
