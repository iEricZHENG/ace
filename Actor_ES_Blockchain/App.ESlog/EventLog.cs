using App.Framework.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.ESLog
{
    public class EventLog<T>
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
        /// 日志详细信息
        /// </summary>
        public string Message
        {
            get;
            set;
        }
        /// <summary>
        /// 日志数据
        /// </summary>
        public T EventData { get; set; }
        /// <summary>
        /// 日志添加时间
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }
    }
}
