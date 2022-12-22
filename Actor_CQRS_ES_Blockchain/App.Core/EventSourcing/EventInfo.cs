namespace App.Core.EventSourcing
{
    public class EventInfo<T>
    {
        public string Id
        {
            get;
            set;
        }
        public bool IsComplete
        {
            get;
            set;
        }
        public IEventBase<T> Event
        {
            get;
            set;
        }
    }
}
