using AssetTrackingService.DataTransferObject;

namespace AssetTrackingService.Abstract
{
    public interface IAssetProductivityService
    {
        Task<int> AddProductivityAsync(AssetProdInsertDto insertDto);
        Task<List<AssetProdReadDto>> GetProductivityAsync();
    }
}
