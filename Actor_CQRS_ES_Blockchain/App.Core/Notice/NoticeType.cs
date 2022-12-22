using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Notice
{
    public enum NoticeType
    {
        /// <summary>
        /// 手机短信都发送
        /// </summary>
        All = 0,
        /// <summary>
        /// 只发送短信
        /// </summary>
        Phone = 1,
        /// <summary>
        /// 只发送邮件
        /// </summary>
        Email = 2,
        /// <summary>
        /// 优先发送手机，如果没有发送邮件
        /// </summary>
        PhoneFirst = 3,
        /// <summary>
        /// 有限发送邮件，如果没有发送短信
        /// </summary>
        EmailFirst = 4
    }
}
