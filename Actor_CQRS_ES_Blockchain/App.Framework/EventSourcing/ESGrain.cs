using App.Core;
using App.Core.EventSourcing;
using App.Core.Message;
using App.Framework.Log;
using App.Framework.MQ;
using Flurl;
using Flurl.Http;
using MongoDB.Bson;
using Newtonsoft.Json;
using Orleans;
using ProtoBuf;
using System.Collections.Concurrent;

namespace App.Framework.EventSourcing
{
    public abstract class ESGrain<S, K> : Grain where S : class, IState<K>, new()
    {
        protected static ConcurrentDictionary<Type, MongoStorageAttribute> mongoAttrDict = new ConcurrentDictionary<Type, MongoStorageAttribute>();
        protected static ConcurrentDictionary<Type, MQPublishAttribute> mqAttrDict = new ConcurrentDictionary<Type, MQPublishAttribute>();
        UInt32 storageVersion; int eventTableVersion;
        protected S State
        {
            get;
            set;
        }
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
        MQPublishAttribute _rabbitMQInfo;
        protected MQPublishAttribute RabbitMQInfo
        {
            get
            {
                if (_rabbitMQInfo == null)
                {
                    var type = this.GetType();
                    if (!mqAttrDict.TryGetValue(type, out _rabbitMQInfo))
                    {
                        _rabbitMQInfo = LoadMQAttr(type);
                        mqAttrDict.TryAdd(type, _rabbitMQInfo);
                    }
                }
                return _rabbitMQInfo;
            }
        }
        protected abstract K GrainId { get; }
        protected virtual bool SendEventToMQ { get { return true; } }

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
        static Type rabbitMQType = typeof(MQPublishAttribute);
        private static MQPublishAttribute LoadMQAttr(Type type)
        {
            var rabbitMQAttributes = type.GetCustomAttributes(rabbitMQType, true);
            MQPublishAttribute mqAttr = null;
            if (rabbitMQAttributes.Length > 0)
            {
                mqAttr = rabbitMQAttributes[0] as MQPublishAttribute;
                if (string.IsNullOrEmpty(mqAttr.Exchange))
                {
                    mqAttr.Exchange = type.Namespace;
                }
                if (string.IsNullOrEmpty(mqAttr.Queue))
                {
                    mqAttr.Queue = type.Name;
                }
            }
            if (mqAttr == null)
            {
                mqAttr = new MQPublishAttribute(type.Namespace, type.Name);
            }
            mqAttr.Init();
            return mqAttr;
        }
        #region LifeTime
        public override Task OnActivateAsync()
        {
            return Active();
        }
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
                        //判断事件消息是否写入成功
                        if (!@event.IsComplete)
                        {
                            await EventInsertAfterHandle(store, @event.Event, @event.Id);
                        }
                    }
                    if (eventList.Count < 1000) break;
                }
            }
        }

        #endregion
        #region State storage
        protected bool IsNew = false;
        protected virtual async Task ReadSnapshotAsync()
        {
            if (SnapshotType != SnapshotType.NoSnapshot)
            {
                var data = await StateStore.GetByIdAsync(GrainId);
                this.State = data.Item1;
                eventTableVersion = data.Item2;
            }
            if (this.State == null)
            {
                IsNew = true;
                await InitState();
            }
            storageVersion = this.State.Version;
        }
        protected virtual SnapshotType SnapshotType { get { return SnapshotType.Master; } }
        protected virtual int SnapshotFrequency { get { return 50; } }
        protected virtual async Task SaveSnapshotAsync()
        {
            if (SnapshotType == SnapshotType.Master)
            {
                if (IsNew)
                {
                    await StateStore.InsertAsync(this.State, eventTableVersion);
                    storageVersion = this.State.Version;
                    IsNew = false;
                }
                //如果版本号差超过设置则更新快照
                else if (this.State.Version - storageVersion >= SnapshotFrequency)
                {
                    await StateStore.UpdateAsync(this.State, eventTableVersion);
                    storageVersion = this.State.Version;
                }
            }
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
        protected async Task ClearStateAsync()
        {
            await StateStore.DeleteAsync(GrainId);
        }
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
        #endregion

        #region Event
        protected async Task<bool> RaiseEvent(IEventBase<K> @event, string msgId = null, string mqId = null)
        {
            try
            {
                @event.StateId = GrainId;
                @event.Version = this.State.Version + 1;
                @event.Timestamp = DateTime.UtcNow.ToUniversalTime();
                @event.Signature = string.Empty;
                @event.Counter = 0;
                var json = JsonConvert.SerializeObject(@event);
                json = json.Replace("\"", "'");
                //获得签名和counter
                //kiwi
                //BlockchainAdapter blockchainAdapter = new BlockchainAdapter();
                //string response = await blockchainAdapter.PostSign(new Message
                //{
                //    data = json,
                //    signature = string.Empty,
                //    counter = @event.Version
                //});
                Message msg = new Message
                {
                    data = json,
                    signature = string.Empty,
                    counter = @event.Version
                };
                var response = await "http://8.218.184.206:85".AppendPathSegment("sign").PostJsonAsync(msg).ReceiveString();

                MessageResponse messageResponse = JsonConvert.DeserializeObject<MessageResponse>(response);

                @event.Signature = messageResponse.signature;
                @event.Counter = messageResponse.counter;

                var store = GetEventStore();
                var result = await store.InsertAsync(@event, msgId, SendEventToMQ);
                if (result.Item1)
                {
                    @event.Apply(this.State);
                    eventTableVersion = store.TableVersion;//保存事件表的版本
                    await EventInsertAfterHandle(store, @event, result.Item2, mqId: mqId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = this.GetMQLogger("event_store");
                await log.Error($"applay event {@event.TypeCode} error, eventId={@event.Version}", ex);
                throw ex;
            }
            return false;
        }
        private async Task EventInsertAfterHandle(IEventStorage<K> store, IEventBase<K> @event, string id, int recursion = 0, string mqId = null)
        {
            try
            {
                if (SendEventToMQ)
                {
                    if (string.IsNullOrEmpty(mqId)) mqId = GrainId.ToString();
                    //消息写入消息队列                  
                    await RabbitMQInfo.SendMsg(@event, mqId);
                }
                //更改消息状态
                await store.Complete(id);
                //保存快照
                await SaveSnapshotAsync();
            }
            catch (Exception e)
            {
                if (recursion > 5) throw e;
                await GetMQLogger().Fatal("事件complate操作出现致命异常:" + string.Format("Grain类型={0},GrainId={1},TableVersion={2},StateId={3},Version={4},错误信息:{5}", this.GetType().FullName, GrainId, store.TableVersion, @event.StateId, @event.Version, e.Message));
                int newRecursion = recursion + 1;
                await Task.Delay(newRecursion * 200);
                await EventInsertAfterHandle(store, @event, id, newRecursion, mqId: mqId);
            }
        }
        /// <summary>
        /// 发送无状态更改的消息到消息队列
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMsg(IActorOwnMessage<K> msg)
        {
            msg.StateId = this.GrainId;
            await RabbitMQInfo.SendMsg(msg, GrainId.ToString());
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
        #endregion
    }
}
