using IoTPlatformLibrary.DataBase.Abstract;

namespace IoTPlatformLibrary.DataBase.Runtime
{
    public class QueryExecuter : IQueryExecuter
    {
        private readonly IDictionary<Type, ICosmosQueryHandler> _builders;

        public QueryExecuter(IEnumerable<ICosmosQueryHandler> queryHandlers)
        {
            _builders = queryHandlers.ToDictionary(builder => builder.HandleQueryType);
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataQuery dataQuery, IRequestOptions requestOptions)
        {
            var destinationType = dataQuery.GetType();

            if (_builders.ContainsKey(destinationType))
            {
                return await _builders[destinationType].HandleAsync<T>(dataQuery, requestOptions);
            }

            throw new Exception($"QueryHandler for type {destinationType} not found");
        }
    }
}
