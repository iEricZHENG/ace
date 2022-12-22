using System.Threading.Tasks;
using System;

namespace App.Core.EventSourcing
{
    public interface IStateStorage<T, K> where T : IState<K>
    {
        Task<Tuple<T, int>> GetByIdAsync(K id);

        Task<string> InsertAsync(T data, int eventTableVersion);

        Task UpdateAsync(T data, int eventTableVersion);

        Task DeleteAsync(K id);
    }
}
