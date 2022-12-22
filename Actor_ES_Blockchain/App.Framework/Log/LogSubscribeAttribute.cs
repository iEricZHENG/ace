using System;
using System.Linq;
using App.Framework.MQ;
using RabbitMQ.Client.Events;
using System.IO;
using Autofac;
using ProtoBuf;
using NLog;

namespace App.Framework.Log
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LogSubscribeAttribute : MQSubscribeAttribute
    {
        static ILogger logger = LogManager.GetCurrentClassLogger(typeof(LogSubscribeAttribute));

        public LogSubscribeAttribute() :
            base(LogManage.Exchange, Enum.GetNames(typeof(LogLevel)).ToList())
        {

        }
    }
}
