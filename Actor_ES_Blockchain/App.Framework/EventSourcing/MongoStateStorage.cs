using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Core.EventSourcing;
using ProtoBuf;
using MongoDB.Driver;
using MongoDB.Bson;
using App.Framework.Db;

namespace App.Framework.EventSourcing
{
    public class MongoStateStorage<T, K> : MongoStorage, IStateStorage<T, K> where T : class, IState<K>
    {
        public MongoStateStorage(string databaseName, string collectionName)
        {
            this.DatabaseName = databaseName;
            this.CollectionName = collectionName;
            CreateIndex();
        }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public async Task DeleteAsync(K id)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", id);
            await GetCollection<BsonDocument>(DatabaseName, CollectionName).DeleteManyAsync(filter);
        }

        private void CreateIndex()
        {
            Task.Run(async () =>
            {
                var collectionService = GetCollection<BsonDocument>(DatabaseName, CollectionName);
                CancellationTokenSource cancel = new CancellationTokenSource(1);
                var index = await collectionService.Indexes.ListAsync();
                var indexList = await index.ToListAsync();
                if (!indexList.Exists(p => p["name"] == "State"))
                {
                    await collectionService.Indexes.CreateOneAsync("{'StateId':1}", new CreateIndexOptions { Name = "State", Unique = true });
                }
            }).ConfigureAwait(false);
        }
        public async Task<Tuple<T, int>> GetByIdAsync(K id)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", id);
            var cursor = await GetCollection<BsonDocument>(DatabaseName, CollectionName).FindAsync<BsonDocument>(filter);
            var document = await cursor.FirstOrDefaultAsync();
            T result = null;
            int eventTableVersion = 0;
            if (document != null)
            {
                var data = document["Data"]?.AsByteArray;
                eventTableVersion = document["EventTableVersion"].AsInt32;
                if (data != null)
                {
                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        result = Serializer.Deserialize<T>(ms);
                    }
                }
            }
            return new Tuple<T, int>(result, eventTableVersion);
        }

        public async Task<string> InsertAsync(T data, int eventTableVersion)
        {
            var mState = new MongoState<K>();
            mState.StateId = data.StateId;
            mState.EventTableVersion = eventTableVersion;
            mState.Id = ObjectId.GenerateNewId().ToString();
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, data);
                mState.Data = ms.ToArray();

            }
            if (mState.Data != null && mState.Data.Count() > 0)
                await GetCollection<MongoState<K>>(DatabaseName, CollectionName).InsertOneAsync(mState, null, new CancellationTokenSource(3000).Token);
            return mState.Id;
        }

        public async Task UpdateAsync(T data, int eventTableVersion)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("StateId", data.StateId);
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<T>(ms, data);
                bytes = ms.ToArray();
            }
            if (bytes != null && bytes.Count() > 0)
            {
                var update = Builders<BsonDocument>.Update.Set("Data", bytes).Set("EventTableVersion", eventTableVersion);
                await GetCollection<BsonDocument>(DatabaseName, CollectionName).UpdateOneAsync(filter, update, null, new CancellationTokenSource(3000).Token);
            }
        }
    }
}
