namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IServiceContext
    {
        string UserName { get; }
        string UserId { get; }
        string TenantId { get; }
        string CorrelationId { get; }
        string Platform { get; }
        string OrganizationId { get; }
        string AccessToken { get; }
    }
}
