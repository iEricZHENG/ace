using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.Notice
{
    /// <summary>
    /// 邮箱账号信息
    /// </summary>
    public class EmailUser
    {
        /// <summary>
        /// 主机
        /// </summary>
        public string Host
        {
            get;
            set;
        }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get;
            set;
        }
        public string From
        {
            get;
            set;
        }
        /// <summary>
        /// 账号
        /// </summary>
        public string Account
        {
            get;
            set;
        }
        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord
        {
            get;
            set;
        }
    }
}
