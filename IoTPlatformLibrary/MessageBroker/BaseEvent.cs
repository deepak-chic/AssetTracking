namespace IoTPlatformLibrary
{
    public abstract class BaseEvent
    {
        public abstract string CreatedBy { get; set; }
        public abstract DateTime CreatedDate { get; set; }

        public abstract Type MessageType { get; }
    }
}
