using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core.EventSourcing;
using ProtoBuf;

namespace App.Framework.Log
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class LogInfo
    {
        /// <summary>
        /// 日志来源应用
        /// </summary>
        public string SourceApp { get; set; }
        /// <summary>
        /// 日志来源信息
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 日志IP
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level
        {
            get;
            set;
        }
        /// <summary>
        /// 日志类型
        /// </summary>
        public string Type
        {
            get;
            set;
        }
        /// <summary>
        /// 日志详细信息
        /// </summary>
        public string Message
        {
            get;
            set;
        }
        /// <summary>
        /// json格式的数据
        /// </summary>
        public string JsonData { get; set; }
        /// <summary>
        /// 日志添加时间
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }
        /// <summary>
        /// Trace信息
        /// </summary>
        public string StackTrace { get; set; }
    }
}
