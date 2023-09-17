namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IRequestOptions
    {
        string PartitionKey { set; get; }

        IEnumerable<string> IncludeEntities { set; get; }

        IServiceContext ServiceContext { set; get; }
    }
}
