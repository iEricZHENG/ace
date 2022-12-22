using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using Autofac;

namespace App.Framework.MQ
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MQSubscribeAttribute : Attribute
    {

        List<QueueInfo> queueList;
        List<string> originQueueList;
        string exchange, queue;
        int queueCount;
        public MQSubscribeAttribute(string exchange, string queue, int queueCount = 1)
        {
            this.exchange = exchange;
            this.queueCount = queueCount;
            this.queue = queue;
            Init();
        }
        public MQSubscribeAttribute(string exchange, List<string> queueList)
        {
            this.exchange = exchange;
            originQueueList = queueList;
            Init();
        }
        QueueInfo BuildQueueInfo(string queue)
        {
            return new QueueInfo()
            {
                RoutingKey = queue,
                Queue = queue
            };
        }
        private void Init()
        {
            this.queueList = new List<QueueInfo>();
            if (originQueueList?.Count > 0)
            {
                foreach (var q in originQueueList)
                {
                    this.queueList.Add(BuildQueueInfo(q));
                }
            }
            else if (!string.IsNullOrEmpty(queue))
            {
                if (queueCount == 1)
                {
                    queueList.Add(BuildQueueInfo(queue));
                }
                else
                {
                    for (int i = 0; i < QueueCount; i++)
                    {
                        queueList.Add(BuildQueueInfo(queue + "_" + i));
                    }
                }
            }
            //申明exchange
            RabbitMQClient.ExchangeDeclare(this.exchange).Wait();

        }
        public List<QueueInfo> QueueList
        {
            get
            {
                return queueList;
            }
        }
        public string Exchange
        {
            get
            {
                return this.exchange;
            }
        }
        public int QueueCount
        {
            get
            {
                return this.queueCount;
            }
        }
    }
    public class QueueInfo
    {
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
    }
}
