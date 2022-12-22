namespace App.Core.Message
{
    public interface IMessage
    {
        string TypeCode { get; }
    }
    public interface IActorOwnMessage<K> : IMessage
    {
        K StateId { get; set; }
    }
}
