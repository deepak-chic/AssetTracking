namespace AssetTrackingService.DataTransferObject
{
    public class AssetProdReadDto : AssetProdInsertDto
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
