namespace IoTPlatformLibrary.ServiceBus
{
    public class ServiceBusConfigOption
    {
        public const string ConfigName = "ServiceBusConfig";
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public int PrefetchCount { get; set; }
        public bool IsSessionEnabled { get; set; }
        public int MaxConcurrentSessions { get; set; }
        public int MaxConcurrentCalls { get; set; } 
        public bool AutoComplete { get; set; }
    }
}
