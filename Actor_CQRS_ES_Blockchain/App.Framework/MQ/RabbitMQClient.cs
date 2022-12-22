using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using RabbitMQ.Client;
using ProtoBuf;
using App.Core;
using App.Core.Message;
using Newtonsoft.Json;

namespace App.Framework.MQ
{
    public class RabbitHost
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public int MaxPoolSize { get; set; }
        public string[] Hosts
        {
            get
            {
                return hosts;
            }
            set
            {
                hosts = value;
                if (hosts != null)
                {
                    list = new List<AmqpTcpEndpoint>();
                    foreach (var host in hosts)
                    {
                        list.Add(AmqpTcpEndpoint.Parse(host));
                    }
                }
            }
        }
        List<AmqpTcpEndpoint> list;
        string[] hosts;
        public List<AmqpTcpEndpoint> EndPoints
        {
            get
            {
                return list;
            }
        }
    }
    public class ConnectionWrapper
    {
        public IConnection Connection { get; set; }
        int modelCount = 0;
        public int ModelCount { get { return modelCount; } }
        public int Increment()
        {
            return Interlocked.Increment(ref modelCount);
        }
        public void Decrement()
        {
            Interlocked.Decrement(ref modelCount);
        }
        public void Reset()
        {
            modelCount = 0;
        }
    }
    public class ModelWrapper : IDisposable
    {
        public IModel Model { get; set; }
        public void Dispose()
        {
            RabbitMQClient.PushModel(this);
        }
    }
    public static class RabbitMQClient
    {
        static RabbitMQClient()
        {
            var connStr = System.Configuration.ConfigurationManager.ConnectionStrings["rabbitmq"].ConnectionString;
            rabbitHost = JsonConvert.DeserializeObject<RabbitHost>(connStr);
            _Factory = new ConnectionFactory()
            {
                UserName = rabbitHost.UserName,
                Password = rabbitHost.Password,
                VirtualHost = rabbitHost.VirtualHost,
                AutomaticRecoveryEnabled = true
            };
        }
        static ConnectionFactory _Factory;
        static RabbitHost rabbitHost;
        /// <summary>
        /// MQPublishAttribute的拓展方法，用来发送事件消息
        /// </summary>
        /// <typeparam name="K">事件消息的类型</typeparam>
        /// <param name="rabbitMQInfo"></param>
        /// <param name="msg">消息</param>
        /// <param name="key">用来做queue的负载均衡，通常为Grain的key</param>
        public static async Task SendMsg(this MQPublishAttribute rabbitMQInfo, IMessage msg, string key)
        {
            var data = new MessageInfo();
            data.TypeCode = msg.TypeCode;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, msg);
                data.Data = ms.ToArray();
            }
            await Send(data, rabbitMQInfo.Exchange, rabbitMQInfo.GetQueue(key));
        }
        public static async Task Send<T>(this MQPublishAttribute rabbitMQInfo, T data, string key)
        {
            await Send(data, rabbitMQInfo.Exchange, rabbitMQInfo.GetQueue(key));
        }
        public static async Task SendCmd<T>(this MQPublishAttribute rabbitMQInfo, UInt16 cmd, T data, string key)
        {
            await SendCmd<T>(cmd, data, rabbitMQInfo.Exchange, rabbitMQInfo.GetQueue(key));
        }
        /// <summary>
        /// 发送消息到消息队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static async Task Send<T>(T data, string exchange, string queue, bool persistent = true)
        {
            byte[] msg;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, data);
                msg = ms.ToArray();
            }
            await Send(msg, exchange, queue, persistent);
        }
        public static async Task SendCmd<T>(UInt16 cmd, T data, string exchange, string queue)
        {
            byte[] msg;
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(cmd), 0, 2);
                Serializer.Serialize(ms, data);
                msg = ms.ToArray();
            }
            await Send(msg, exchange, queue, false);
        }
        public static async Task Send(byte[] msg, string exchange, string queue, bool persistent = true)
        {
            using (var channel = await PullModel())
            {
                var prop = channel.Model.CreateBasicProperties();
                prop.Persistent = persistent;
                channel.Model.BasicPublish(exchange, queue, prop, msg);
                channel.Model.WaitForConfirmsOrDie();
            }
        }
        public static async Task ExchangeDeclare(string exchange)
        {
            using (var channel = await PullModel())
            {
                channel.Model.ExchangeDeclare(exchange, "direct", true);
            }
        }
        static ConcurrentQueue<ModelWrapper> modelPool = new ConcurrentQueue<ModelWrapper>();
        static ConcurrentQueue<TaskCompletionSource<ModelWrapper>> modelTaskPool = new ConcurrentQueue<TaskCompletionSource<ModelWrapper>>();
        static ConcurrentBag<ConnectionWrapper> connectionList = new ConcurrentBag<ConnectionWrapper>();
        static int connectionCount = 0;
        static object modelLock = new object();
        public static IConnection CreateConnection()
        {
            return _Factory.CreateConnection(rabbitHost.EndPoints);
        }
        public static async Task<ModelWrapper> PullModel()
        {
            if (!modelPool.TryDequeue(out var model))
            {
                ConnectionWrapper conn = null;
                foreach (var item in connectionList)
                {
                    if (item.Increment() <= 10)
                    {
                        conn = item;
                        break;
                    }
                    else
                    {
                        item.Decrement();
                    }
                }
                if (conn == null && Interlocked.Increment(ref connectionCount) <= rabbitHost.MaxPoolSize)
                {
                    conn = new ConnectionWrapper() { Connection = _Factory.CreateConnection(rabbitHost.EndPoints) };
                    conn.Connection.ConnectionShutdown += (obj, args) =>
                    {
                        conn.Connection = _Factory.CreateConnection(rabbitHost.EndPoints);
                        conn.Reset();
                    };
                    connectionList.Add(conn);
                }
                if (conn != null)
                {
                    model = new ModelWrapper() { Model = conn.Connection.CreateModel() };
                    model.Model.ConfirmSelect();
                }
                else
                {
                    Interlocked.Decrement(ref connectionCount);
                }

            }
            if (model == null)
            {
                var taskSource = new TaskCompletionSource<ModelWrapper>();
                modelTaskPool.Enqueue(taskSource);
                var cancelSource = new CancellationTokenSource(3000);
                cancelSource.Token.Register(() =>
                {
                    taskSource.SetException(new Exception("获取rabbitmq的model超时"));
                });
                model = await taskSource.Task;
            }
            if (model.Model.IsClosed)
            {
                model = await PullModel();
            }
            return model;
        }
        public static void PushModel(ModelWrapper model)
        {
            if (model.Model.IsOpen)
            {
                TaskCompletionSource<ModelWrapper> modelTask;
                if (modelTaskPool.TryDequeue(out modelTask))
                {
                    if (modelTask.Task.IsCanceled)
                        PushModel(model);
                    modelTask.SetResult(model);
                }
                else
                    modelPool.Enqueue(model);
            }
            else
            {
                model.Model.Dispose();
            }
        }
    }
}
