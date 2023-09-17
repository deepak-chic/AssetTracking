using AutoMapper;

namespace AssetTrackingService.AutoMapper
{
    /// <summary>
    /// Data mapper registration.
    /// </summary>
    public static class DataMapperRegistration
    {
        /// <summary>
        /// Adds the Entity/Domain automapper profile configurations.
        /// </summary>
        /// <param name="configuration">A <see cref="IMapperConfigurationExpression"/> to add the profiles to.</param>
        public static void ConfigureDataProfiles(this IMapperConfigurationExpression configuration)
        {
            configuration.AddProfile<AssetProductivityMappingProfile>();
        }
    }
}
