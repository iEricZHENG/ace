using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using App.Framework.Db;

namespace App.Framework.EventSourcing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoStorageAttribute : Attribute
    {
        public string EventDataBase { get; set; }
        public string EventCollection { get; set; }
        public string SnapshotDataBase { get; set; }
        public string SnapshotCollection { get; set; }
        public string DenormalizeSnapshotCollection { get; set; }

        const string C_CName = "CollectionInfo";
        bool sharding = false;
        int shardingDays;
        public MongoStorageAttribute(
            string eventDatabase, string collection, bool sharding = false, int shardingDays = 90, string snapshotDatabase = null)
        {
            this.EventDataBase = eventDatabase;
            this.EventCollection = collection + "Event";
            this.SnapshotCollection = collection + "State";
            this.DenormalizeSnapshotCollection = collection + "DenormalizeState";
            this.sharding = sharding;
            this.shardingDays = shardingDays;
            if (!string.IsNullOrEmpty(snapshotDatabase))
                this.SnapshotDataBase = snapshotDatabase;
            else
                this.SnapshotDataBase = eventDatabase;
            CreateIndex();//创建索引
        }
        public List<CollectionInfo> GetCollectionList(int version = 0)
        {
            List<CollectionInfo> list = null;
            if (version == 0)
                list = GetAllCollectionList();
            else
            {
                list = GetAllCollectionList().Where(c => c.Version >= version).ToList();
            }
            if (list == null)
            {
                list = new List<CollectionInfo>() { GetCollection() };
            }
            return list;
        }
        private List<CollectionInfo> collectionList;
        public List<CollectionInfo> GetAllCollectionList()
        {
            if (collectionList == null)
            {
                lock (collectionLock)
                {
                    if (collectionList == null)
                    {
                        collectionList = MongoStorage.GetCollection<CollectionInfo>(EventDataBase, C_CName).Find<CollectionInfo>(c => c.Type == EventCollection).ToList();
                    }
                }
            }
            return collectionList;
        }
        private void CreateIndex()
        {
            Task.Run(async () =>
            {
                var collectionService = MongoStorage.GetCollection<BsonDocument>(EventDataBase, C_CName);
                CancellationTokenSource cancel = new CancellationTokenSource(1);
                var index = await collectionService.Indexes.ListAsync();
                var indexList = await index.ToListAsync();
                if (!indexList.Exists(p => p["name"] == "Name"))
                {
                    await collectionService.Indexes.CreateOneAsync("{'Name':1}", new CreateIndexOptions { Name = "Name", Unique = true });
                }
            }).ConfigureAwait(false);
        }
        object collectionLock = new object();
        static DateTime startTime = new DateTime(2017, 11, 8);
        public CollectionInfo GetCollection()
        {
            CollectionInfo lastCollection = null;
            var cList = GetAllCollectionList();
            if (cList.Count > 0) lastCollection = cList.Last();
            //如果不需要分表，直接返回
            if (lastCollection != null && !this.sharding) return lastCollection;
            var subTime = DateTime.UtcNow.Subtract(startTime);
            var cVersion = subTime.TotalDays > 0 ? Convert.ToInt32(Math.Floor(subTime.TotalDays / shardingDays)) : 0;
            if (lastCollection == null || cVersion > lastCollection.Version)
            {
                lock (collectionLock)
                {
                    if (lastCollection == null || cVersion > lastCollection.Version)
                    {
                        var collection = new CollectionInfo();
                        collection.Id = ObjectId.GenerateNewId().ToString();
                        collection.Version = cVersion;
                        collection.Type = EventCollection;
                        collection.CreateTime = DateTime.UtcNow;
                        collection.Name = EventCollection + "_" + cVersion;
                        try
                        {
                            MongoStorage.GetCollection<CollectionInfo>(EventDataBase, C_CName).InsertOne(collection);
                            collectionList.Add(collection);
                            lastCollection = collection;
                        }
                        catch (MongoWriteException ex)
                        {
                            if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                            {
                                collectionList = null;
                                return GetCollection();
                            }
                        }
                    }
                }
            }
            return lastCollection;
        }
    }
}
