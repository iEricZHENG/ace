using App.Core.Message;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;
using System.IO;
using ProtoBuf;
using App.Core.EventSourcing;
using App.Framework.Log;
using System.Collections.Concurrent;

namespace App.Framework.EventSourcing
{
    public abstract class ESReplicaGrain<S, K> : Grain where S : class, IState<K>, new()
    {
        protected static ConcurrentDictionary<Type, MongoStorageAttribute> mongoAttrDict = new ConcurrentDictionary<Type, MongoStorageAttribute>();
        UInt32 storageVersion; int eventTableVersion;
        protected S State
        {
            get;
            set;
        }
        protected abstract K GrainId { get; }

        protected MongoStorageAttribute _mongoInfo;
        protected virtual MongoStorageAttribute ESMongoInfo
        {
            get
            {
                if (_mongoInfo == null)
                {
                    var type = this.GetType();
                    if (!mongoAttrDict.TryGetValue(type, out _mongoInfo))
                    {
                        _mongoInfo = LoadMongoAttr(type);
                        mongoAttrDict.TryAdd(type, _mongoInfo);
                    }
                }
                return _mongoInfo;
            }
        }
        static Type eventStorageType = typeof(MongoStorageAttribute);
        private static MongoStorageAttribute LoadMongoAttr(Type type)
        {
            var mongoStorageAttributes = type.GetCustomAttributes(eventStorageType, true);
            if (mongoStorageAttributes.Length > 0)
            {
                return mongoStorageAttributes[0] as MongoStorageAttribute;
            }
            return new MongoStorageAttribute("EventSourcing", type.FullName);
        }
        protected virtual SnapshotType SnapshotType { get { return SnapshotType.Replica; } }
        protected virtual int SnapshotFrequency { get { return 50; } }
        public async Task Execute(MessageInfo message)
        {
            var type = MessageTypeMapping.GetType(message.TypeCode);
            if (type != null)
            {
                using (var ems = new MemoryStream(message.Data))
                {
                    var @event = Serializer.Deserialize(type, ems) as IEventBase<K>;
                    if (@event != null)
                    {
                        if (@event.Version == this.State.Version + 1)
                        {
                            await OnExecution(@event);
                            @event.Apply(State);
                            await OnExecuted(@event);
                            await SaveSnapshotAsync();
                        }
                        else if (@event.Version > this.State.Version)
                        {
                            var collectionList = ESMongoInfo.GetCollectionList();
                            foreach (var collection in collectionList)
                            {
                                var store = GetEventStore(collection);
                                var eventList = await store.GetListAsync(this.GrainId, this.State.Version, @event.Version);
                                foreach (var item in eventList)
                                {
                                    await OnExecution(@event);
                                    item.Event.Apply(State);
                                    await OnExecuted(@event);
                                    await SaveSnapshotAsync();
                                }
                                if (this.State.Version >= @event.Version) break;
                            }
                        }
                        if (@event.Version == this.State.Version + 1)
                        {
                            await OnExecution(@event);
                            @event.Apply(State);
                            await OnExecuted(@event);
                            await SaveSnapshotAsync();
                        }
                        if (@event.Version > this.State.Version)
                        {
                            throw new Exception($"ESReplicaGrain出现严重BUG,Type={this.GetType().FullName},StateId={this.GrainId.ToString()},StateVersion={this.State.Version},EventVersion={@event.Version}");
                        }
                    }
                }
            }
        }
        protected virtual Task OnExecuted(IEventBase<K> @event)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnExecution(IEventBase<K> @event)
        {
            return Task.CompletedTask;
        }
        public override async Task OnActivateAsync()
        {
            await Active();
        }
        protected virtual Task CustomSave()
        {
            return Task.CompletedTask;
        }
        protected virtual async Task SaveSnapshotAsync()
        {
            if (SnapshotType == SnapshotType.Replica)
            {
                if (this.State.Version - storageVersion >= SnapshotFrequency)
                {
                    await CustomSave();//自定义保存项
                    if (IsNew)
                    {
                        await StateStore.InsertAsync(this.State, eventTableVersion);
                        IsNew = false;
                    }
                    else
                    {
                        await StateStore.UpdateAsync(this.State, eventTableVersion);
                    }
                    storageVersion = this.State.Version;
                }
            }
        }
        #region 初始化数据
        private async Task Active()
        {
            await ReadSnapshotAsync();
            var collectionList = ESMongoInfo.GetCollectionList(eventTableVersion);
            foreach (var collection in collectionList)
            {
                var store = GetEventStore(collection);
                while (true)
                {
                    var eventList = await store.GetListAsync(this.GrainId, this.State.Version, this.State.Version + 1000);
                    foreach (var @event in eventList)
                    {
                        @event.Event.Apply(this.State);
                        eventTableVersion = store.TableVersion;//保存事件表的版本
                    }
                    if (eventList.Count < 1000) break;
                }
            }
        }
        protected bool IsNew = false;
        protected virtual async Task ReadSnapshotAsync()
        {
            var data = await StateStore.GetByIdAsync(GrainId);
            this.State = data.Item1;
            eventTableVersion = data.Item2;
            if (this.State == null)
            {
                IsNew = true;
                await InitState();
            }
            storageVersion = this.State.Version;
        }
        /// <summary>
        /// 初始化状态，必须实现
        /// </summary>
        /// <returns></returns>
        protected virtual Task InitState()
        {
            this.State = new S();
            this.State.StateId = GrainId;
            return Task.CompletedTask;
        }
        #endregion
        #region Storage
        IStateStorage<S, K> _StateStore;
        private IStateStorage<S, K> StateStore
        {
            get
            {
                if (_StateStore == null)
                {
                    _StateStore = new MongoStateStorage<S, K>(ESMongoInfo.SnapshotDataBase, ESMongoInfo.SnapshotCollection);
                }
                return _StateStore;
            }
        }
        Dictionary<string, IEventStorage<K>> eventStoreDict = new Dictionary<string, IEventStorage<K>>();
        private IEventStorage<K> GetEventStore(CollectionInfo collection = null)
        {
            if (collection == null)
            {
                collection = ESMongoInfo.GetCollection();
            }
            IEventStorage<K> _EventStore;
            if (!eventStoreDict.TryGetValue(collection.Name, out _EventStore))
            {
                _EventStore = new MongoEventStorage<K>(ESMongoInfo.EventDataBase, collection);
                eventStoreDict.Add(collection.Name, _EventStore);
            }
            return _EventStore;
        }
        #endregion
        #region MQ
        protected virtual ILogger GetMQLogger(string source = null, string appName = null)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return LogManage.GetLogger(source, appName);
            }
            return LogManage.GetLogger(this.GetType(), appName);
        }
        #endregion
    }
}
