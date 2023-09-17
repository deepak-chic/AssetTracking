using AssetTrackingService.Abstract;
using AssetTrackingService.DataTransferObject;
using AutoMapper;
using IoTPlatformLibrary.EventHandler;
using IoTPlatformLibrary.Events;

namespace AssetTrackingService.EventHandler
{
    public class TelemetryMessageHandler : BaseEventHandler<TelemetryMessage>
    {
        private readonly IAssetProductivityService _assetProductivityService;
        private readonly IMapper _mapper;
        public Type MessageType = typeof(TelemetryMessage);

        public TelemetryMessageHandler(IAssetProductivityService assetProductivityService, IMapper mapper)
        {
            _assetProductivityService = assetProductivityService;
            _mapper = mapper;
        }

        public override async Task HandleEvent(TelemetryMessage message)
        {
            var insertDto = _mapper.Map<AssetProdInsertDto>(message);
            await _assetProductivityService.AddProductivityAsync(insertDto);
        }
    }
}
