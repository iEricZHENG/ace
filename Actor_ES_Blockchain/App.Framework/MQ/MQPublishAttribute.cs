using System;
using System.Collections.Generic;
using App.Core.Lib;

namespace App.Framework.MQ
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MQPublishAttribute : Attribute
    {
        public MQPublishAttribute(string exchange = null, string queue = null, int queueCount = 1)
        {
            this.Exchange = exchange;
            this.Queue = queue;
            this.QueueCount = queueCount;
        }
        ConsistentHash _CHash;
        List<string> nodeList = new List<string>();
        public void Init()
        {
            for (int i = 0; i < QueueCount; i++)
            {
                nodeList.Add(Queue + "_" + i);
            }
            _CHash = new ConsistentHash(nodeList, QueueCount * 10);
            //申明exchange
            RabbitMQClient.ExchangeDeclare(this.Exchange).Wait();
        }
        public string GetQueue(string key)
        {
            if (QueueCount == 1) return Queue;

            return _CHash.GetNode(key);
        }
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public int QueueCount { get; set; }
    }
}
