using Newtonsoft.Json;
using App.Core.Ioc;
using App.Framework;
using App.Framework.Db;
using App.IActor;
using App.IActor.Repository;
using Dapper;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Repository
{
    [IocExport(typeof(IAccountRepository), true)]
    public class AccountRepository : IAccountRepository
    {
        public async Task<AccountState> GetAsync(string id)
        {
            int userid = int.Parse(id);
            using (var conn = PSQLDbBase.GetConnection().Base)
            {
                string sql = @"SELECT Id StateId,Balance,Version FROM Account WHERE Id=@userid";
                return await conn.QueryFirstOrDefaultAsync<AccountState>(sql, new { userid = userid });
            }
        }

        public async Task<int> WaitWriteToBlockchainAsync(Message message)
        {
            using (var conn = PSQLDbBase.GetConnection().Base)
            {
                string sql = @"INSERT INTO data(data) VALUES(@data)";
                string json =Newtonsoft.Json.JsonConvert.SerializeObject(message);
                return await conn.ExecuteAsync(sql, new { data = json });
            }
        }

        public async Task<int> WriteToDbAsync()
        {
            using (var conn = PSQLDbBase.GetConnection().Base)
            {
                string sql = @"INSERT INTO Account(Balance,Version) VALUES(@Balance,0)";
                return await conn.ExecuteAsync(sql, new { Balance = 1 });
            }
        }
    }
}
