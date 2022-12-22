using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Notice
{
    public interface IEmail
    {
        MailMessage CreateMessage(List<string> addressList, string body, string title);
        void Attachments(string Path, MailMessage mailMessage);
        /// <summary>
        /// 异步发送邮件
        /// </summary>
        Task SendAsync(MailMessage mailMessage);
        /// <summary>
        /// 发送邮件
        /// </summary>
        void Send(MailMessage mailMessage);
    }
}
