using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Log
{
    public enum LogLevel
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info = 0,
        /// <summary>
        /// 提醒
        /// </summary>
        Warning = 1,
        /// <summary>
        /// 跟踪
        /// </summary>
        Trace = 2,
        /// <summary>
        /// BUG
        /// </summary>
        Debug = 3,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 4,
        /// <summary>
        /// 致命问题
        /// </summary>
        Fatal = 5
    }
}
