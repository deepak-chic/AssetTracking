namespace IoTPlatformLibrary.Events
{
    public class TelemetryMessage: BaseEvent
    {
        public string DeviceId { get; set; }
        public string TenantId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? EngineTemprature { get; set; }
        public int Counter { get; set; }
        public override Type MessageType => typeof(TelemetryMessage);

        public override string CreatedBy { get; set; }
        public override DateTime CreatedDate  { get; set; } = DateTime.UtcNow;
    }
}
