using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Framework.EventSourcing
{
    public interface IReplicaGrain
    {
        Task Tell(MessageInfo message);
    }
}
