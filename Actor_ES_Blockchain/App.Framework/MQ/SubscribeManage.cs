using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using App.Framework.Log;
using App.Core.Lib;

namespace App.Framework.MQ
{
    public class SubscribeManage
    {
        static Type mqSubscribeType = typeof(MQSubscribeAttribute);
        static Core.ILogger logger = LogManage.GetLogger(typeof(SubscribeManage));
        static SubscribeManage()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.IndexOf("App") != -1);
            var eventType = typeof(ISubscribeHandler);
            handDict.Clear();
            foreach (var assembly in assemblyList)
            {
                var allType = assembly.GetExportedTypes().Where(t => eventType.IsAssignableFrom(t)).ToList();
                foreach (var type in allType)
                {
                    var attributes = type.GetCustomAttributes(mqSubscribeType, true).ToList();
                    if (attributes.Count > 0)
                    {
                        foreach (var attr in attributes)
                        {
                            var mqAttr = attr as MQSubscribeAttribute;
                            if (mqAttr != null)
                            {
                                builder.RegisterType(type).Named(type.FullName, eventType).SingleInstance();
                                handDict.Add(new KeyValuePair<Type, MQSubscribeAttribute>(type, mqAttr));
                            }
                        }
                    }
                }
            }
            container = builder.Build();
        }
        static ContainerBuilder builder = new ContainerBuilder();
        static IContainer container;
        static List<KeyValuePair<Type, MQSubscribeAttribute>> handDict = new List<KeyValuePair<Type, MQSubscribeAttribute>>();
        static ConcurrentBag<ConsumerInfo> ConsumerAllList = new ConcurrentBag<ConsumerInfo>();

        public static async Task Start(string node, List<string> nodeList = null)
        {
            var hash = nodeList == null ? null : new ConsistentHash(nodeList);
            var consumerList = new List<ConsumerInfo>();
            foreach (var hand in handDict)
            {
                for (int i = 0; i < hand.Value.QueueList.Count(); i++)
                {
                    var queue = hand.Value.QueueList[i];
                    var hashNode = hash != null ? hash.GetNode(queue.Queue) : node;
                    if (node == hashNode)
                    {
                        consumerList.Add(new ConsumerInfo() { Exchange = hand.Value.Exchange, Queue = node + "_" + queue.Queue, RoutingKey = queue.RoutingKey, HandlerName = hand.Key.FullName });
                    }
                }
            }
            await Start(consumerList);
        }
        public static async Task Start(List<ConsumerInfo> consumerList)
        {
            if (consumerList != null)
            {
                var channel = await RabbitMQClient.PullModel();

                for (int i = 0; i < consumerList.Count; i++)
                {
                    var consumer = consumerList[i];
                    consumer.Channel = channel;
                    channel.Model.ExchangeDeclare(consumer.Exchange, "direct", true);
                    channel.Model.QueueDeclare(consumer.Queue, true, false, false, null);
                    channel.Model.QueueBind(consumer.Queue, consumer.Exchange, consumer.RoutingKey);
                    consumer.BasicConsumer = new EventingBasicConsumer(consumer.Channel.Model);
                    consumer.BasicConsumer.Received += (ch, ea) =>
                    {
                        try
                        {
                            var handle = container.ResolveNamed<ISubscribeHandler>(consumer.HandlerName);
                            handle.Notice(ea.Body).ContinueWith(t =>
                            {
                                if (t.Exception == null && !t.IsCanceled)
                                {
                                    consumer.Channel.Model.BasicAck(ea.DeliveryTag, false);
                                }
                                else if (t.Exception != null)
                                {
                                    throw t.Exception;
                                }
                                else if (t.IsCanceled)
                                {
                                    throw new Exception("消息处理超时");
                                }
                            }).GetAwaiter().GetResult();
                        }
                        catch (Exception exception)
                        {
                            //需要记录错误日志
                            var e = exception.InnerException ?? exception;
                            logger.Fatal(string.Format("消息队列消息处理失败，失败的队列为{0}-{1} , ConsumerTag为:{2} , 处理的handle为{3}",
                                consumer.Exchange, consumer.Queue, ea.ConsumerTag, consumer.HandlerName), e);
                            ReStart(consumer);//重启队列
                        }
                    };
                    consumer.BasicConsumer.ConsumerTag = consumer.Channel.Model.BasicConsume(consumer.Queue, false, consumer.BasicConsumer);
                    if (i % 4 == 0 && i != 0)
                    {
                        channel = await RabbitMQClient.PullModel();
                    }
                    if (!ConsumerAllList.Contains(consumer))
                    {
                        ConsumerAllList.Add(consumer);
                    }
                }
            }
        }
        /// <summary>
        /// 重启消费者
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        public static void ReStart(ConsumerInfo consumer)
        {
            if (consumer.Channel.Model.IsOpen)
            {
                consumer.Channel.Model.BasicCancel(consumer.BasicConsumer.ConsumerTag);
                consumer.BasicConsumer.ConsumerTag = consumer.Channel.Model.BasicConsume(consumer.Queue, false, consumer.BasicConsumer);
            }
        }
    }
    public class ConsumerInfo
    {
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
        public string HandlerName { get; set; }
        public ModelWrapper Channel { get; set; }
        public EventingBasicConsumer BasicConsumer { get; set; }
    }
}
