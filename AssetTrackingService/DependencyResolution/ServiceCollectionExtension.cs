using AssetTrackingService.Abstract;
using AssetTrackingService.AutoMapper;
using AssetTrackingService.EventHandler;
using AssetTrackingService.Services;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using IoTPlatformLibrary.EventHandler;
using IoTPlatformLibrary.ServiceBus;

namespace AssetTrackingService.DependencyResolution
{
    public static class ServiceCollectionExtension
    {
        public static void AddServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            // Automapper registration
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.ConfigureDataProfiles();
                cfg.AddExpressionMapping();
            });
            mapperConfiguration.CompileMappings();
            mapperConfiguration.AssertConfigurationIsValid();
            services.AddAutoMapper(x =>
            {
                x.ConfigureDataProfiles();
                x.AddExpressionMapping();
            });

            // Service Bus injection
            services.Configure<ServiceBusConfigOption>(configuration.GetSection(ServiceBusConfigOption.ConfigName));
            services.AddSingleton(typeof(IIntegrationBaseHandler), typeof(TelemetryMessageHandler));
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumer>();
            services.AddSingleton<IAssetProductivityService, AssetProductivityService>();
        }
    }
}
