using AssetTrackingService.DataTransferObject;

namespace AssetTrackingService.Abstract
{
    public interface IAssetProductivityService
    {
        Task<Guid> AddProductivityAsync(AssetProdInsertDto insertDto);
        Task<List<AssetProdReadDto>> GetProductivityAsync();
    }
}
