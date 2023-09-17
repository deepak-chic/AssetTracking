namespace AssetTrackingService.DataTransferObject
{
    public class AssetProdInsertDto
    {
        public string DeviceId { get; set; }
        public string TenantId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? EngineTemprature { get; set; }
    }
}
