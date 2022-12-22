using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Framework.EventSourcing
{
    public class MongoState<K>
    {
        [BsonId]
        public string Id { get; set; }
        public K StateId { get; set; }
        public int EventTableVersion { get; set; }
        public byte[] Data { get; set; }
    }
}
