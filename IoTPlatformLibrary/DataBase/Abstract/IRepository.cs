using System.Linq.Expressions;

namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IRepository<TDomain, TKey> : IQueryable<TDomain>
        where TDomain : class, IDomainEntity<TKey>
    {
        Task<bool> IsExistAsync(TKey id, IRequestOptions requestOptions);
        Task<TDomain> GetByIdAsync(TKey id, IRequestOptions requestOptions);
        Task<TDomain> GetLastAsync(IServiceContext serviceContext = null);
        Task<IEnumerable<TDomain>> GetAllAsync(IRequestOptions requestOptions);
        Task<IEnumerable<TDomain>> Find(Expression<Func<TDomain, bool>> filterPredicate);
        Task<TDomain> AddAsync(TDomain entity, IServiceContext serviceContext);
        Task<TDomain> UpdateAsync(TDomain entity, IServiceContext serviceContext);
        Task<TDomain> UpSertAsync(TDomain entity, IServiceContext serviceContext);
        Task DeleteAsync(TKey id, IServiceContext serviceContext);
        Task DeleteAsync(TDomain entity, IServiceContext serviceContext);
        Task<IEnumerable<TDomain>> ReadAsync(IDataQuery query, IServiceContext serviceContext);
        Task<IEnumerable<TResult>> ReadAsync<TResult>(IDataQuery query, IServiceContext serviceContext);
    }
}
