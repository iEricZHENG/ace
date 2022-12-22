using App.Core.EventSourcing;
using ProtoBuf;

namespace App.IActor
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AccountState:IState<string>
    {
        public AccountState()
        {
        }     
        public decimal Balance { get; set; }
        public string StateId { get; set; }
        public UInt32 Version { get; set; }
    }
}