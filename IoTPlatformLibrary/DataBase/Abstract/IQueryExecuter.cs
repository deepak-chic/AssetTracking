namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IQueryExecuter
    {
        Task<IEnumerable<T>> ExecuteQuery<T>(IDataQuery dataQuery, IRequestOptions requestOptions);
    }
}
