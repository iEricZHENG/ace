using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using App.Core;
using Newtonsoft.Json;
using NLog.Config;

namespace App.Framework.Log
{
    public class MQLogger : ILogger
    {
        static NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger(typeof(MQLogger));
        static MQLogger()
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
        string appName, source, exchange;
        LogLevel level;
        public MQLogger(string exchange, string appName, string source, LogLevel level)
        {
            this.appName = appName;
            this.source = source;
            this.level = level;
            this.exchange = exchange;
            MQ.RabbitMQClient.ExchangeDeclare(exchange).Wait();
        }
        private async Task Publish(LogInfo data)
        {
            try
            {
                await MQ.RabbitMQClient.Send(data, exchange, data.Level.ToString(), false);
            }
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.InnerExceptions)
                    logger.Fatal(exception.InnerException);
            }
        }
        #region debug
        public async Task Debug(string message)
        {
            var log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug(Exception e)
        {
            var log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = e.Message;
            log.StackTrace = e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug(string message, Exception e)
        {
            var log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.StackTrace = e.Message + ",Trace:" + e.StackTrace;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug<T>(string message, T data)
        {
            var log = new LogInfo();
            log.Level = LogLevel.Debug;
            log.Message = message;
            log.IPAddress = ipAddress;
            await Log(log);
        }

        public async Task Debug<T>(Exception e, T data)
        {
            var log = new LogInfo();
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
                info.Source = source;
                info.SourceApp = appName;
                info.Time = DateTime.UtcNow;
                if (data != null)
                {
                    info.JsonData = JsonConvert.SerializeObject(data);
                }
                await Publish(info);
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
