using AssetTrackingService.Abstract;
using AssetTrackingService.DataTransferObject;
using AssetTrackingService.Entities;
using AutoMapper;
using IoTPlatformLibrary;
using IoTPlatformLibrary.DataBase.Abstract;
using IoTPlatformLibrary.DataBase.Runtime;
using Microsoft.Azure.Amqp.Framing;

namespace AssetTrackingService.Services
{
    public class AssetProductivityService : IAssetProductivityService
    {
        private readonly IRepository<AssetProductivity, Guid> _assetProductivityRepo;
        private readonly IMapper _mapper;

        public AssetProductivityService(IRepository<AssetProductivity, Guid> assetProductivityRepo, IMapper mapper)
        {
            _assetProductivityRepo = assetProductivityRepo;
            _mapper = mapper;
        }

        public async Task<Guid> AddProductivityAsync(AssetProdInsertDto insertDto)
        {
            var entity = _mapper.Map<AssetProductivity>(insertDto);
            entity.id = Guid.NewGuid();
            var response = await _assetProductivityRepo.AddAsync(entity, new ServiceContext());
            return response.id;
        }

        public async Task<List<AssetProdReadDto>> GetProductivityAsync()
        {
            var response = await _assetProductivityRepo.GetAllAsync(new RequestOptions(new ServiceContext()));
            var items = _mapper.Map<List<AssetProdReadDto>>(response);
            return items;
        }
    }
}
