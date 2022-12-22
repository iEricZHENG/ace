using App.Core;
using App.Core.Message;
using App.Framework.MQ;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace App.Framework.MQ
{
    public abstract class MsgSubscribeHandler : ISubscribeHandler
    {
        public abstract Task Notice(MessageInfo message, object data);

        public virtual Task Notice(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var msg = Serializer.Deserialize<MessageInfo>(ms);
                var type = MessageTypeMapping.GetType(msg.TypeCode);
                if (type == null)
                {
                    throw new Exception("TypeCode为" + msg.TypeCode + "的Type不存在");
                }
                using (var ems = new MemoryStream(msg.Data))
                {
                    return this.Notice(msg, Serializer.Deserialize(type, ems));
                }
            }
        }

        public async Task Send(object request, string exchange, string queue)
        {
            var data = new MessageInfo();
            data.TypeCode = request.GetType().FullName;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, request);
                data.Data = ms.ToArray();
            }
            await RabbitMQClient.Send(data, exchange, queue);
        }
    }
}
