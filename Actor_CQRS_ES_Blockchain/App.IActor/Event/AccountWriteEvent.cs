using App.Core.EventSourcing;
using ProtoBuf;
using System;

namespace App.IActor.Event
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AccountWriteEvent : IEventBase<string>
    {
        public AccountWriteEvent()
        {

        }
        public AccountWriteEvent(string commandId, string userId, decimal increase)
        {
            this.CommandId = commandId;
            this.StateId = userId;
            this.Increase = increase;
        }

        public string Id { get; set; }
        public uint Version { get; set; }
        public string CommandId { get; set; }
        public DateTime Timestamp { get; set; }
        public string StateId { get; set; }
        private static string _TypeCode = typeof(AccountWriteEvent).FullName;
        [ProtoIgnore]
        public string TypeCode { get { return _TypeCode; } }


        public decimal Increase { get; set; }
        public string Signature { get; set; }
        public uint Counter { get; set; }

        public void Apply(IState<string> state)
        {
            var modelState = state as AccountState;
            if (modelState != null)
            {
                this.ApplyBase(modelState);
                modelState.StateId = this.StateId;
                modelState.Balance = modelState.Balance + this.Increase;
            }
        }
    }
}
