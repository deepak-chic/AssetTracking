using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IDomainEntity<TKey> : IDomainEntity
    {
        TKey id { get; set; }
    }

    public interface IDomainEntity
    {
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string TenantId { get; set; }
        string PartitionKey { get; set; }
        string ConcurrencyToken { get; set; } 
    }
}
