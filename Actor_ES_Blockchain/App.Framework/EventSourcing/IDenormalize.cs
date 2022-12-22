using App.Core;
using System.Threading.Tasks;

namespace App.Framework.EventSourcing
{
    public interface IDenormalize
    {
        Task Tell(MessageInfo msg);
    }
}
