using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.EventSourcing;
using App.Framework.MQ;
using ProtoBuf;
using App.Core;
using System.IO;
using App.Core.Message;

namespace App.Framework.EventSourcing
{
    public abstract class ToActorEventSubscribeHandler<K> : ISubscribeHandler
    {
        public virtual async Task Notice(MessageInfo message, object data)
        {
            var @event = data as IActorOwnMessage<K>;
            if (@event != null)
            {
                await Tell(@event, message);
                //如果handle不存在则丢弃消息
            }
        }

        public Task Notice(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var msg = Serializer.Deserialize<MessageInfo>(ms);
                var type = MessageTypeMapping.GetType(msg.TypeCode);
                if (!string.IsNullOrEmpty(msg.TypeCode))
                {
                    if (type == null)
                    {
                        throw new Exception("TypeCode为" + msg.TypeCode + "的Type不存在");
                    }
                    using (var ems = new MemoryStream(msg.Data))
                    {
                        return this.Notice(msg, Serializer.Deserialize(type, ems));
                    }
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
        }

        public abstract Task Tell(IActorOwnMessage<K> @event, MessageInfo msg);
    }
}
