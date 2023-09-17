using IoTPlatformLibrary.DataBase.Abstract;

namespace IoTPlatformLibrary.DataBase.Runtime
{
    public class RequestOptions : IRequestOptions
    {
        public string PartitionKey { set; get; }

        public IEnumerable<string> IncludeEntities { set; get; }

        public IServiceContext ServiceContext { set; get; }

        public RequestOptions(IServiceContext serviceContext) : this(serviceContext, string.Empty)
        {

        }
        public RequestOptions(IServiceContext serviceContext, string partitionKey)
        {
            ServiceContext = serviceContext;
            PartitionKey = partitionKey;
        }
    }
}
