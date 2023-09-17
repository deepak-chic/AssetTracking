using IoTPlatformLibrary.DataBase.Runtime;

namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IDocumentHelper
    {
        Task<TEntity> AddAsync<TEntity>(TEntity entity, IRequestOptions requestOptions);
        Task DeleteAsync<TEntity>(string id, string partitionKey, IRequestOptions requestOptions);
        Task<IEnumerable<TEntity>> ExecuteSqlAsync<TEntity>(string sqlQuery, IRequestOptions requestOptions);
        Task<TEntity> UpSertAsync<TEntity>(TEntity entity, IRequestOptions requestOptions);
        Task<List<dynamic>> GetDocumentsAsync(string sqlQuery, string collectionName, IRequestOptions requestOptions);
        Task<List<T>> GetDocumentsAsync<T>(string sqlQuery, string collectionName, IRequestOptions requestOptions);
        EntityConfigurationOptions GetEntityConfigurationOptions(Type entityType);
        Dictionary<string, EntityConfigurationOptions> GetEntityConfigurationForAll();
    }
}
