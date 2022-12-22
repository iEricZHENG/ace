using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Framework.EventSourcing
{
    public class MongoEvent<K>
    {
        [BsonId]
        public string Id { get; set; }
        public K StateId
        {
            get;
            set;
        }
        public string MsgId { get; set; }
        public UInt32 Version
        {
            get;
            set;
        }

        public string TypeCode
        {
            get;
            set;
        }
        public bool IsComplete
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }
    }
}
