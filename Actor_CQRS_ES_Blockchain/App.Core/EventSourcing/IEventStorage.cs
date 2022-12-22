using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace App.Core.EventSourcing
{
    public interface IEventStorage<K>
    {
        int TableVersion { get; }
        Task<List<EventInfo<K>>> GetListAsync(K stateId, UInt32 startVersion);
        Task<List<EventInfo<K>>> GetListAsync(K stateId, UInt32 startVersion, UInt32 endVersion);
        Task<List<EventInfo<K>>> GetListAsync(K stateId, string typeCode, UInt32 startVersion);
        Task<List<EventInfo<K>>> GetListAsync<T>(string typeCode, UInt32 startVersion, UInt32 endVersion);
        Task<Tuple<bool, string>> InsertAsync<T>(T data, string msgId = null, bool needComplate = true) where T : IEventBase<K>;
        Task UpdateAsync<T>(T data) where T : IEventBase<K>;
        Task Complete(string id);
        Task DeleteAsync(string teamId, int version);
    }
}
