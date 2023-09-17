using AssetTrackingService.Entities;
using IoTPlatformLibrary.DataBase.Abstract;
using IoTPlatformLibrary.DataBase.Runtime;

namespace AssetTrackingService.Configuration
{
    public class AssetProductivityConfiguration : IEntityTypeConfiguration<AssetProductivity>
    {
        public void Configure(EntityTypeBuilder<AssetProductivity> builder)
        {
            builder.HasPartitionKey(x => x.DeviceId)
                .ToCollection("AssetProductivity");
        }
    }
}
