using System;

namespace App.Core.EventSourcing
{
    public interface IState<K>
    {
        K StateId { get; set; }
        UInt32 Version { get; set; }
    }
}
