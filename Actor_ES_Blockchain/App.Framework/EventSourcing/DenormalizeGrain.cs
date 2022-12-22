using App.Core;
using App.Core.EventSourcing;
using App.Core.Message;
using App.Framework.Log;
using Orleans;
using ProtoBuf;
using System.Collections.Concurrent;

namespace App.Framework.EventSourcing
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DenormalizeState<K> : IState<K>
    {
        public K StateId
        {
            get;
            set;
        }

        public UInt32 Version
        {
            get;
            set;
        }
    }
    public abstract class DenormalizeGrain<K> : Grain
    {
        protected static ConcurrentDictionary<Type, MongoStorageAttribute> mongoAttrDict = new ConcurrentDictionary<Type, MongoStorageAttribute>();
        protected DenormalizeState<K> State
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
        protected List<string> outsideMsgTypecodeList = new List<string>();
        protected void DeclareOutsideMsg(string typeCode)
        {
            outsideMsgTypecodeList.Add(typeCode);
        }
        public async Task Tell(MessageInfo msg)
        {
            var type = MessageTypeMapping.GetType(msg.TypeCode);
            if (type != null)
            {
                using (var ems = new MemoryStream(msg.Data))
                {
                    var message = Serializer.Deserialize(type, ems);
                    if (message != null)
                    {
                        if (!outsideMsgTypecodeList.Contains(msg.TypeCode))
                        {
                            if (message is IEvent @event)
                            {
                                if (@event.Version == this.State.Version + 1)
                                {
                                    await ProcessEvent(@event);
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
                                            await ProcessEvent(item.Event);
                                        }
                                        if (this.State.Version >= @event.Version) break;
                                    }
                                }
                            }
                        }
                        else if (message is IMessage value)
                        {
                            await ProcessMsg(value);
                        }
                    }
                }
            }
        }
        protected Task UpdateVersion(IEvent @event)
        {
            this.State.Version = @event.Version;
            if (@event.Version % 100 == 0)
            {
                return SaveSnapshotAsync();
            }
            else
                return Task.CompletedTask;
        }
        protected async Task ProcessEvent(IEvent @event)
        {
            var ts = new TaskCompletionSource<bool>();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Run(async () =>
            {
                await Execute(@event).ContinueWith(t =>
                {
                    if (t.Exception == null)
                    {
                        return UpdateVersion(@event);
                    }
                    else
                    {
                        if (t.Exception.InnerException is Npgsql.PostgresException e && e.SqlState == "23505")
                        {
                            return UpdateVersion(@event);
                        }
                        throw t.Exception.InnerException;
                    }
                }).ContinueWith(t =>
                {
                    if (t.Exception == null && !t.IsCanceled)
                    {
                        ts.TrySetResult(true);
                    }
                    else if (t.IsCanceled)
                    {
                        ts.TrySetCanceled();
                    }
                    else
                        ts.TrySetException(t.Exception.InnerException);
                });
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

            await ts.Task;
        }
        protected async Task ProcessMsg(IMessage msg)
        {
            var ts = new TaskCompletionSource<bool>();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Run(async () =>
            {
                await Execute(msg).ContinueWith(t =>
                {
                    if (t.Exception == null && !t.IsCanceled)
                    {
                        ts.TrySetResult(true);
                    }
                    else
                    {
                        if (t.Exception.InnerException is Npgsql.PostgresException e)
                        {
                            if (e.SqlState == "23505")
                                ts.TrySetResult(true);
                            else
                                ts.TrySetException(t.Exception.InnerException);
                        }
                        else
                            ts.TrySetException(t.Exception.InnerException);
                    }
                });
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            await ts.Task;
        }
        protected virtual Task Execute(IMessage msg)
        {
            return Task.CompletedTask;
        }
        #region LifeTime
        public override Task OnActivateAsync()
        {
            return Active();
        }
        private async Task Active()
        {
            await ReadSnapshotAsync();
        }
        #endregion
        #region State storage
        protected bool IsNew = false;
        protected async Task ReadSnapshotAsync()
        {
            var data = await StateStore.GetByIdAsync(GrainId);
            this.State = data.Item1;
            if (this.State == null)
            {
                IsNew = true;
                await InitState();
            }
        }
        protected async Task SaveSnapshotAsync()
        {
            if (IsNew)
            {
                await StateStore.InsertAsync(this.State, 0);
                IsNew = false;
            }
            else
            {
                await StateStore.UpdateAsync(this.State, 0);
            }
        }
        /// <summary>
        /// 初始化状态，必须实现
        /// </summary>
        /// <returns></returns>
        protected virtual Task InitState()
        {
            this.State = new DenormalizeState<K>();
            this.State.StateId = GrainId;
            return Task.CompletedTask;
        }
        IStateStorage<DenormalizeState<K>, K> _StateStore;
        private IStateStorage<DenormalizeState<K>, K> StateStore
        {
            get
            {
                if (_StateStore == null)
                {
                    _StateStore = new MongoStateStorage<DenormalizeState<K>, K>(ESMongoInfo.SnapshotDataBase, ESMongoInfo.DenormalizeSnapshotCollection);
                }
                return _StateStore;
            }
        }
        Dictionary<string, IEventStorage<K>> eventStoreDict = new Dictionary<string, IEventStorage<K>>();
        protected IEventStorage<K> GetEventStore(CollectionInfo collection = null)
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
