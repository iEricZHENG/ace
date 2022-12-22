using App.Core.Ioc;
using App.Core;
using App.Framework.EventSourcing;
using App.Framework.MQ;
using App.IActor;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using App.IActor.Repository;
using App.Repository;
using App.IActor.Event;
using App.Core.Lib;

namespace App.Actor
{
    [MQPublish("App.Actor", "Account", 5)]
    public class Account : ESGrain<AccountState, string>, IAccount
    {
        protected override string GrainId => this.GetPrimaryKeyString();
        protected override MongoStorageAttribute ESMongoInfo
        {
            get
            {
                if (_mongoInfo == null)
                {
                    _mongoInfo = new MongoStorageAttribute("Account_" + this.GrainId.Split('_')[0] + "_EventStore", "AccountWrite", true);
                }
                return _mongoInfo;
            }
        }
        protected override async Task InitState()
        {
            var repository = new AccountRepository();
            this.State = await SQLTask.SQLTaskExecute(() => repository.GetAsync(this.GrainId));

            if (this.State == null)
            {
                await base.InitState();
            }
            else
            {
                this.State.Version = 0;
                await this.SaveSnapshotAsync();
            }
        }
        public Task<decimal> Read()
        {
            return Task.FromResult(this.State.Balance);
        }

        public async Task Write()
        {
            AccountWriteEvent @event = new AccountWriteEvent(OGuid.GenerateNewId().ToString(), "1", 1);
            await this.RaiseEvent(@event);
            //return Task.FromResult<decimal>(1);
        }
    }
}
