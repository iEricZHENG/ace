using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using App.Core.Notice;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Concurrent;

namespace App.Framework.Notice
{
    public class Email : IEmail
    {
        public EmailUser account;
        public Email(EmailUser user)
        {
            account = user;
        }
        static Email()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };
        }
        private SmtpClient Client
        {
            get
            {
                if (!ClientPool.TryDequeue(out var client))
                {
                    client = new SmtpClient();
                    client.Credentials = new System.Net.NetworkCredential(account.Account, account.PassWord);//设置发件人身份的票据  
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Host = account.Host;
                    if (account.Port != 0)
                        client.Port = account.Port;
                    else
                        client.Port = 587;
                    client.EnableSsl = true;
                    client.SendCompleted += (sender, e) =>
                    {
                        ClientPool.Enqueue(client);
                    };
                }
                return client;
            }
        }
        static ConcurrentQueue<SmtpClient> ClientPool = new ConcurrentQueue<SmtpClient>();
        public void Attachments(string Path, MailMessage mailMessage)
        {
            string[] path = Path.Split(',');
            Attachment data;
            ContentDisposition disposition;
            for (int i = 0; i < path.Length; i++)
            {
                data = new Attachment(path[i], MediaTypeNames.Application.Octet);//实例化附件  
                disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(path[i]);//获取附件的创建日期  
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(path[i]);// 获取附件的修改日期  
                disposition.ReadDate = System.IO.File.GetLastAccessTime(path[i]);//获取附件的读取日期  
                mailMessage.Attachments.Add(data);//添加到附件中  
            }
        }

        public MailMessage CreateMessage(List<string> addressList, string body, string title)
        {
            var mailMessage = new MailMessage();
            foreach (var t in addressList)
            {
                mailMessage.To.Add(t);
            }
            mailMessage.From = new System.Net.Mail.MailAddress(account.From);
            mailMessage.Subject = title;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码  
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.Priority = System.Net.Mail.MailPriority.High;
            return mailMessage;
        }

        public void Send(MailMessage mailMessage)
        {
            if (mailMessage != null)
            {

                Client.Send(mailMessage);
            }
        }

        public async Task SendAsync(MailMessage mailMessage)
        {
            if (mailMessage != null)
            {
                await Client.SendMailAsync(mailMessage);
            }
        }
    }
}
