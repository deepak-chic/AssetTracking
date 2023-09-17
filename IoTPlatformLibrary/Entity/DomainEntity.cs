using IoTPlatformLibrary.DataBase.Abstract;

namespace IoTPlatformLibrary.Entity
{
    public abstract class DomainEntity<TKey> : IDomainEntity<TKey>
    {
        public TKey id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string TenantId { get; set; }
        public string PartitionKey { get; set; }
        public string ConcurrencyToken { get; set; }
    }
}
