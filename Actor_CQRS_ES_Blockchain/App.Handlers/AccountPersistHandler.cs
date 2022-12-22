using App.Core;
using App.Core.Ioc;
using App.Framework;
using App.Framework.MQ;
using App.IActor.Event;
using App.IActor.Repository;
using App.Repository;
using Autofac;
using Newtonsoft.Json;
using System.Diagnostics;

namespace App.Handlers
{
    [MQSubscribe("App.Actor", "Account", 5)]
    public class AccountPersistHandler : MsgSubscribeHandler
    {
        public override Task Notice(MessageInfo message, object data)
        {
            switch (data)
            {
                case AccountWriteEvent value: return AccountWriteAsync(value);

                default: return Task.CompletedTask;
            }
        }

        private async Task AccountWriteAsync(AccountWriteEvent value)
        {
            //await Task.Delay(10*1000);//模拟异步时间
            //上链-kiwi
            var accountRepository = IocManage.Container.Resolve<IAccountRepository>();
            //await accountRepository.WriteToDbAsync();
            Message tx = new Message();
            value.Timestamp = DateTime.Parse($"{value.Timestamp.ToString("o")}Z").ToUniversalTime();
            tx.signature = value.Signature;
            tx.counter = value.Counter;
            value.Signature = String.Empty;
            value.Id = null;
            value.Counter = 0;
            string json = JsonConvert.SerializeObject(value);
            json = json.Replace("\"", "'");
            tx.data = json;
            //string str = await accountRepository.WriteToBlockchainAsync(tx);
            BlockchainAdapter blockchainAdapter = new BlockchainAdapter();
            //string str = await blockchainAdapter.PostUpload(tx);
            await accountRepository.WaitWriteToBlockchainAsync(tx);
        }
    }
}