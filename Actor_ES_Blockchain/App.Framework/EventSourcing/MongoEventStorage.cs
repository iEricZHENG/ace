using App.Core;
using App.Core.EventSourcing;
using App.Core.Message;
using App.Framework.Db;
using App.Framework.Log;
using MongoDB.Bson;
using MongoDB.Driver;
using ProtoBuf;

namespace App.Framework.EventSourcing
{
    public class MongoEventStorage<K> : MongoStorage, IEventStorage<K>
    {
        static ILogger ILogger = LogManage.GetLogger("MongoEventStorage");
        public MongoEventStorage(string databaseName, string collectionName)
        {
            this.DatabaseName = databaseName;
            this.CollectionName = collectionName;
            CreateIndex();//自动创建索引
        }
        public MongoEventStorage(string databaseName, CollectionInfo collection)
        {
            this.DatabaseName = databaseName;
            this.Collection = collection;
            CreateIndex();//自动创建索引
        }
        private string collectionName = null;
        public string DatabaseName { get; set; }
        public CollectionInfo Collection { get; set; }
        public string CollectionName
        {
            get
            {
                if (this.Collection != null)
                {
                    return this.Collection.Name;
                }
                return this.collectionName;
            }
            set
            {
                this.collectionName = value;
            }
        }
        public int TableVersion
        {
            get
            {
                return Collection.Version;
            }
        }
        private void CreateIndex()
        {
            var collectionService = GetCollection<BsonDocument>(DatabaseName, CollectionName);
            var indexList = collectionService.Indexes.List().ToList();
            if (!indexList.Exists(p => p["name"] == "State_Version") && !indexList.Exists(p => p["name"] == "State_MsgId"))
            {
                collectionService.Indexes.CreateMany(
                    new List<CreateIndexModel<BsonDocument>>() {
                new CreateIndexModel<BsonDocument>("{'StateId':1,'Version':1}", new CreateIndexOptions { Name = "State_Version",Unique=true }),
                new CreateIndexModel<BsonDocument>("{'StateId':1,'TypeCode':1,'MsgId':1}", new CreateIndexOptions { Name = "State_MsgId", Unique = true }) }
                    );
            }
        }
        public async Task DeleteAsync(string stateId, int version)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", stateId) & filterBuilder.Gt("Version", version - 1);
            await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).DeleteManyAsync(filter);
        }

        public async Task<List<EventInfo<K>>> GetListAsync(K stateId, UInt32 startVersion)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", stateId) & filterBuilder.Gt("Version", startVersion);
            var cursor = await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).FindAsync<BsonDocument>(filter, cancellationToken: new CancellationTokenSource(3000).Token);
            var list = new List<EventInfo<K>>();
            foreach (var document in cursor.ToEnumerable())
            {
                var typeCode = document["TypeCode"].AsString;
                var type = MessageTypeMapping.GetType(typeCode);
                var data = document["Data"].AsByteArray;
                var eventInfo = new EventInfo<K>();
                eventInfo.IsComplete = document["IsComplete"].AsBoolean;
                eventInfo.Id = document["_id"].AsString;
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var @event = Serializer.Deserialize(type, ms) as IEventBase<K>;
                    eventInfo.Event = @event;
                }
                list.Add(eventInfo);
            }
            return list.OrderBy(e => e.Event.Version).ToList();
        }
        public async Task<List<EventInfo<K>>> GetListAsync(K stateId, UInt32 startVersion, UInt32 endVersion)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", stateId) & filterBuilder.Lte("Version", endVersion) & filterBuilder.Gt("Version", startVersion);
            var cursor = await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).FindAsync<BsonDocument>(filter, cancellationToken: new CancellationTokenSource(3000).Token);
            var list = new List<EventInfo<K>>();
            foreach (var document in cursor.ToEnumerable())
            {
                var typeCode = document["TypeCode"].AsString;
                var type = MessageTypeMapping.GetType(typeCode);
                var data = document["Data"].AsByteArray;
                var eventInfo = new EventInfo<K>();
                eventInfo.IsComplete = document["IsComplete"].AsBoolean;
                eventInfo.Id = document["_id"].AsString;
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var @event = Serializer.Deserialize(type, ms) as IEventBase<K>;
                    eventInfo.Event = @event;
                }
                list.Add(eventInfo);
            }
            return list.OrderBy(e => e.Event.Version).ToList();
        }
        public async Task<List<EventInfo<K>>> GetListAsync(K stateId, string typeCode, UInt32 startVersion)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", stateId) & filterBuilder.Eq("TypeCode", typeCode) & filterBuilder.Gt("Version", startVersion);
            var cursor = await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).FindAsync<BsonDocument>(filter, cancellationToken: new CancellationTokenSource(3000).Token);
            var list = new List<EventInfo<K>>();
            foreach (var document in cursor.ToEnumerable())
            {
                var type = MessageTypeMapping.GetType(typeCode);
                var data = document["Data"].AsByteArray;
                var eventInfo = new EventInfo<K>();
                eventInfo.IsComplete = document["IsComplete"].AsBoolean;
                eventInfo.Id = document["_id"].AsString;
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var @event = Serializer.Deserialize(type, ms) as IEventBase<K>;
                    eventInfo.Event = @event;
                }
                list.Add(eventInfo);
            }
            return list.OrderBy(e => e.Event.Version).ToList();
        }

        public async Task<List<EventInfo<K>>> GetListAsync<T>(string typeCode, UInt32 startVersion, UInt32 endVersion)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("TypeCode", typeCode) & filterBuilder.Lte("Version", endVersion) & filterBuilder.Gt("Version", startVersion);
            var cursor = await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).FindAsync<BsonDocument>(filter, cancellationToken: new CancellationTokenSource(3000).Token);
            var list = new List<EventInfo<K>>();
            foreach (var document in cursor.ToEnumerable())
            {
                var data = document["Data"].AsByteArray;
                var eventInfo = new EventInfo<K>();
                eventInfo.IsComplete = document["IsComplete"].AsBoolean;
                eventInfo.Id = document["_id"].AsString;
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var @event = Serializer.Deserialize(typeof(T), ms) as IEventBase<K>;
                    eventInfo.Event = @event;
                }
                list.Add(eventInfo);
            }
            return list.OrderBy(e => e.Event.Version).ToList();
        }

        public async Task<Tuple<bool, string>> InsertAsync<T>(T data, string msgId = null, bool needComplate = true) where T : IEventBase<K>
        {
            var mEvent = new MongoEvent<K>();
            mEvent.StateId = data.StateId;
            mEvent.Version = data.Version;
            mEvent.TypeCode = data.TypeCode;
            if (string.IsNullOrEmpty(data.Id))
            {
                mEvent.Id = ObjectId.GenerateNewId().ToString();
                data.Id = mEvent.Id;
            }
            else
            {
                mEvent.Id = data.Id;
            }

            mEvent.IsComplete = !needComplate;

            bool sucess = false;
            if (string.IsNullOrEmpty(msgId))
                mEvent.MsgId = mEvent.Id;
            else
                mEvent.MsgId = msgId;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, data);
                mEvent.Data = ms.ToArray();
            }
            try
            {
                await GetCollection<MongoEvent<K>>(DatabaseName, this.CollectionName).InsertOneAsync(mEvent);
                sucess = true;
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category != ServerErrorCategory.DuplicateKey)
                {
                    throw ex;
                }
                else
                {
                    await ILogger.Info($"事件重复插入,Event:{Newtonsoft.Json.JsonConvert.SerializeObject(data)}");
                }
            }
            return new Tuple<bool, string>(sucess, mEvent.Id);
        }
        public async Task UpdateAsync<T>(T data) where T : IEventBase<K>
        {
            byte[] byteData = null;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, data);
                byteData = ms.ToArray();
            }
            var update = Builders<MongoEvent<K>>.Update.Set("Data", byteData);
            await GetCollection<MongoEvent<K>>(DatabaseName, this.CollectionName).UpdateOneAsync<MongoEvent<K>>(me => me.Id == data.Id, update);
        }

        public async Task Complete(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var update = Builders<BsonDocument>.Update.Set("IsComplete", true);
            await GetCollection<BsonDocument>(DatabaseName, this.CollectionName).UpdateOneAsync(filter, update);
        }
    }
}
