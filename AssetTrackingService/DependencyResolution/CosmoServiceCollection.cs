using AssetTrackingService.Configuration;
using AssetTrackingService.Entities;
using IoTPlatformLibrary.DataBase;
using IoTPlatformLibrary.DataBase.Abstract;
using IoTPlatformLibrary.DataBase.Runtime;

namespace AssetTrackingService.DependencyResolution
{
    public static class CosmoServiceCollection
    {
        public static void AddCosmoService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IQueryExecuter, QueryExecuter>();            
            services.AddSingleton(typeof(IEntityTypeConfiguration), typeof(AssetProductivityConfiguration));
            services.AddSingleton(typeof(IEntityTypeConfiguration<AssetProductivity>), typeof(AssetProductivityConfiguration));
            services.AddSingleton<IDocumentHelper, DocumentHelper>();
            services.Configure<CosomoConfigOption>(configuration.GetSection(CosomoConfigOption.ConfigName));
            services.AddSingleton(typeof(IRepository<,>), typeof(CosmoRepository<,>));
        }
    }
}
