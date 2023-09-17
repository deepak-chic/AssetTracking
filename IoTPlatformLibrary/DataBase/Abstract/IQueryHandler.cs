namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IQueryHandler
    {
        Type HandleQueryType { get; }
        Task<IEnumerable<T>> HandleAsync<T>(IDataQuery args, IRequestOptions requestOptions);
    }
}
