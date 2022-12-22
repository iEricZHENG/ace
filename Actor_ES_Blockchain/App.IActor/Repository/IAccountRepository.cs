using App.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.IActor.Repository
{
    public interface IAccountRepository
    {
        Task<AccountState> GetAsync(string id);
        Task<int> WriteToDbAsync();
        Task<int> WaitWriteToBlockchainAsync(Message message);
    }
}
