using AssetTrackingService.DataTransferObject;
using AssetTrackingService.Entities;
using AutoMapper;
using IoTPlatformLibrary.Events;

namespace AssetTrackingService.AutoMapper
{
    public class AssetProductivityMappingProfile : Profile
    {
        public AssetProductivityMappingProfile()
        {
            CreateMap<TelemetryMessage, AssetProdInsertDto>();
            CreateMap<AssetProdInsertDto, AssetProductivity>(MemberList.Source);                
            CreateMap<AssetProductivity, AssetProdReadDto>();
        }
    }
}
