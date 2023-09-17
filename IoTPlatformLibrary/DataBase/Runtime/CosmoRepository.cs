using System.Collections;
using System.Linq.Expressions;
using IoTPlatformLibrary.DataBase.Abstract;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IoTPlatformLibrary.DataBase.Runtime
{
    public partial class CosmoRepository<TDomainEntity, TKey> : IRepository<TDomainEntity, TKey>
          where TDomainEntity : class, IDomainEntity<TKey>, new()
    {
        private readonly IDocumentHelper _documentHelper;
        private readonly ILogger<CosmoRepository<TDomainEntity, TKey>> _logger;
        IQueryExecuter _queryExecuter;
        EntityConfigurationOptions _entityConfigurationOptions;

        public CosmoRepository(
            IQueryExecuter queryExecuter,
            IDocumentHelper documentHelper,
            IEntityTypeConfiguration<TDomainEntity> entityTypeConfiguration,
            ILogger<CosmoRepository<TDomainEntity, TKey>> logger)
        {
            _queryExecuter = queryExecuter;
            _documentHelper = documentHelper;
            _logger = logger;
            var entityTypeBuilder = new EntityTypeBuilder<TDomainEntity>();
            entityTypeConfiguration.Configure(entityTypeBuilder);
            _entityConfigurationOptions = entityTypeBuilder.GetConfiguration();
        }

        public async Task<bool> IsExistAsync(TKey id, IRequestOptions requestOptions)
        {
            return await GetByIdAsync(id, requestOptions) != null;
        }

        public async Task<TDomainEntity> GetByIdAsync(TKey id, IRequestOptions requestOptions)
        {
            string partitionKeyCondition = GeneratePartitionKeyFilter(requestOptions);

            if (!string.IsNullOrWhiteSpace(partitionKeyCondition))
                partitionKeyCondition = $" {partitionKeyCondition} and ";

            //TODO: will work only for string type ids 
            return (await _documentHelper.ExecuteSqlAsync<TDomainEntity>($"SELECT * FROM c where {partitionKeyCondition} c.id=\"{id}\"", requestOptions)).FirstOrDefault();
        }

        public async Task<TDomainEntity> GetLastAsync(IServiceContext serviceContext = null)
        {
            return (await _documentHelper.ExecuteSqlAsync<TDomainEntity>("SELECT top 1 * FROM c order by c._ts desc", null)).FirstOrDefault();
        }

        public async Task<IEnumerable<TDomainEntity>> GetAllAsync(IRequestOptions requestOptions)
        {
            string partitionKeyCondition = GeneratePartitionKeyFilter(requestOptions);

            if (!string.IsNullOrWhiteSpace(partitionKeyCondition))
                partitionKeyCondition = $"where {partitionKeyCondition}";

            return await _documentHelper.ExecuteSqlAsync<TDomainEntity>($"SELECT * FROM c {partitionKeyCondition}", requestOptions);
        }

        private string GeneratePartitionKeyFilter(IRequestOptions requestOptions)
        {
            string partitionKeyCondition = string.Empty;

            if (string.IsNullOrWhiteSpace(requestOptions.PartitionKey) && string.IsNullOrWhiteSpace(requestOptions.ServiceContext.TenantId))
                return partitionKeyCondition;

            if (string.IsNullOrWhiteSpace(requestOptions.PartitionKey))
            {
                //assuming PartitionKey will always have TenantId as first key or only key
                if (_entityConfigurationOptions.PartitionKeys.Count > 1)
                    partitionKeyCondition = $"STARTSWITH(c.PartitionKey,'{requestOptions.ServiceContext.TenantId}')";
                else
                    partitionKeyCondition = $"c.PartitionKey='{requestOptions.ServiceContext.TenantId}'";

            }
            else
            {
                partitionKeyCondition = $"PartitionKey={requestOptions.PartitionKey}";
            }

            return partitionKeyCondition;
        }

        public Task<IEnumerable<TDomainEntity>> Find(Expression<Func<TDomainEntity, bool>> filterPredicate)
        {
            throw new NotImplementedException();
        }

        public async Task<TDomainEntity> AddAsync(TDomainEntity entity, IServiceContext serviceContext)
        {
            SetPartitionKey(entity);
            return await _documentHelper.AddAsync(entity, new RequestOptions(serviceContext));
        }

        public async Task<TDomainEntity> UpdateAsync(TDomainEntity entity, IServiceContext serviceContext)
        {
            string entityChange = JsonConvert.SerializeObject(entity);
           // _logger.LogInformation("Updating entity {Name} where all the fields are: {entityChange}  [ServiceContext: {serviceContext}]", typeof(TDomainEntity).Name, entityChange, serviceContext.AsString);

            //TODO: add bool validateReferentialIntegrity = true
            SetPartitionKey(entity);
            return await _documentHelper.UpSertAsync(entity, new RequestOptions(serviceContext));
        }

        //TODO: need add validateReferentialIntegrity check in documentHelper
        public async Task<TDomainEntity> UpSertAsync(TDomainEntity entity, IServiceContext serviceContext)
        {
            string entityChange = JsonConvert.SerializeObject(entity);
           // _logger.LogInformation("Updating entity {Name} where all the fields are: {entityChange} [ServiceContext: {serviceContext}]", typeof(TDomainEntity).Name, entityChange, serviceContext.AsString);
            SetPartitionKey(entity);
            return await _documentHelper.UpSertAsync(entity, new RequestOptions(serviceContext));
        }

        public async Task DeleteAsync(TDomainEntity entity, IServiceContext serviceContext)
        {
            SetPartitionKey(entity);

            await _documentHelper.DeleteAsync<TDomainEntity>(entity.id.ToString(), entity.PartitionKey, new RequestOptions(serviceContext));
        }

        public async Task DeleteAsync(TKey id, IServiceContext serviceContext)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TDomainEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TDomainEntity>> ReadAsync(IDataQuery query, IServiceContext serviceContext)
        {
            return await _queryExecuter.ExecuteQuery<TDomainEntity>(query, new RequestOptions(serviceContext));
        }

        public async Task<IEnumerable<TResult>> ReadAsync<TResult>(IDataQuery query, IServiceContext serviceContext)
        {
            return await _queryExecuter.ExecuteQuery<TResult>(query, new RequestOptions(serviceContext));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Type ElementType => throw new NotImplementedException();
        public Expression Expression => throw new NotImplementedException();
        public IQueryProvider Provider => throw new NotImplementedException();

        private void SetPartitionKey(TDomainEntity entity)
        {
            var syntheticKey = "";

            foreach (var partitionKey in _entityConfigurationOptions.PartitionKeys)
            {
                syntheticKey += GetPropValue(entity, partitionKey).ToString() + "-";
            }

            entity.PartitionKey = syntheticKey.TrimStart('-').TrimEnd('-');
        }

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }
}
