namespace IoTPlatformLibrary.EventHandler
{
    public interface IIntegrationBaseHandler
    {
        public Type MessageType { get; }
    }

    public interface IBaseEventHandler<T> : IIntegrationBaseHandler
        where T : BaseEvent
    {
        public Task HandleEvent(T message);
    }

    public abstract class BaseEventHandler<T> : IBaseEventHandler<T>
        where T : BaseEvent
    {
        public Type MessageType => typeof(T);

        public abstract Task HandleEvent(T message);
    }

}
