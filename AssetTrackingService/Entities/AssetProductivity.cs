using IoTPlatformLibrary.Entity;

namespace AssetTrackingService.Entities
{
    public class AssetProductivity : DomainEntity<Guid>
    {
        public string DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? EngineTemprature { get; set; }
    }
}
