using IoTPlatformLibrary.DataBase.Abstract;

namespace IoTPlatformLibrary
{
    public class ServiceContext : IServiceContext
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string TenantId { get; set; }

        public string CorrelationId { get; set; }

        public string Platform { get; set; }

        public string OrganizationId { get; set; }

        public string AccessToken { get; set; }
    }
}
