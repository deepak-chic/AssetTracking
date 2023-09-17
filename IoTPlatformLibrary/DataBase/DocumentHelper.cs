using IoTPlatformLibrary.DataBase.Abstract;
using IoTPlatformLibrary.DataBase.Runtime;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using MAC = Microsoft.Azure.Cosmos;

namespace IoTPlatformLibrary.DataBase
{
    public sealed class DocumentHelper : IDocumentHelper
    {
        private readonly DocumentClient _documentClient;
        private readonly string _dataBaseName;
        private readonly ILogger<DocumentHelper> _logger;
        bool disposed = false;
        Dictionary<string, EntityConfigurationOptions> _entityConfigLookUp = new Dictionary<string, EntityConfigurationOptions>();
        MAC.CosmosClient _cosmosClient;

        public DocumentHelper(IOptions<CosomoConfigOption> config, IEnumerable<IEntityTypeConfiguration> entityTypeConfigurations, ILogger<DocumentHelper> logger)
        {
            _logger = logger;
            _cosmosClient = new MAC.CosmosClient($"AccountEndpoint={config.Value.AccountEndpoint}/;AccountKey={config.Value.AccountKey};");

            _dataBaseName = config.Value.DataBase;
            _documentClient = new DocumentClient(
                    config.Value.AccountEndpoint,
                    config.Value.AccountKey,
                     new JsonSerializerSettings()
                     {
                         NullValueHandling = NullValueHandling.Ignore,
                         //TODO: need to change json serilzation to camel casing
                         // ContractResolver = new CamelCasePropertyNamesContractResolver()
                     },
                    ConnectionPolicy.Default,
                    ConsistencyLevel.Session
                   );

            SetEntityConfigurationOptions(entityTypeConfigurations);
        }

        public async Task<IEnumerable<TEntity>> ExecuteSqlAsync<TEntity>(string sqlQuery, IRequestOptions requestOptions)
        {
            var entityConfigurationOptions = GetEntityConfigurationOptions(typeof(TEntity));
            var documents = await GetDocumentsAsync(sqlQuery, entityConfigurationOptions.CollectionName, requestOptions);
            var entities = new List<TEntity>();

            if (documents.Any())
            {
                foreach (var doc in documents)
                {
                    try
                    {
                        var entity = JsonConvert.DeserializeObject<TEntity>(doc.ToString());
                        ((IDomainEntity)entity).ConcurrencyToken = doc._etag;

                        entities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while executing SQL statement, query: {Query}, requestOptions: {Options}", sqlQuery, requestOptions);
                    }
                }

                //var referentialIntegrityHelper = new ReferentialIntegrityHelper<TEntity>(this, requestOptions.ServiceContext);

                //await referentialIntegrityHelper.PopulateReferentialObjectsAsync(entities.Cast<object>(), entityConfigurationOptions);
            }

            return entities.Count > 0 ? entities.AsEnumerable() : Enumerable.Empty<TEntity>();
        }

        public async Task<List<dynamic>> GetDocumentsAsync(string sqlQuery, string collectionName, IRequestOptions requestOptions)
        {
            var queryOptions = new FeedOptions();
            queryOptions.EnableCrossPartitionQuery = true;
            queryOptions.EnableScanInQuery = true;
            queryOptions.MaxItemCount = 1000;

            string requestContinuation = string.Empty;
            IDocumentQuery<dynamic> documentQuery = null;
            var documents = new List<dynamic>();
            double totalRUs = 0;
            var startTime = DateTime.UtcNow;

            do
            {
                documentQuery = _documentClient.CreateDocumentQuery<dynamic>(
                  CreateCollectionLink(collectionName),
                 new SqlQuerySpec(sqlQuery),
                  queryOptions).AsDocumentQuery();

                var queryResult = await documentQuery.ExecuteNextAsync<dynamic>();
                totalRUs += queryResult.RequestCharge;

                if (queryResult.ToList().Any())
                {
                    documents.AddRange(queryResult.ToList());
                }

                try
                {
                    requestContinuation = queryResult.ResponseContinuation;
                    queryOptions.RequestContinuation = queryResult.ResponseContinuation;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while retrieving SQL documents, query: {Query}, collection: {Collection}, requestOptions: {Options}", 
                        sqlQuery, collectionName, requestOptions);
                }
            }
            while (!string.IsNullOrEmpty(requestContinuation));

           // AddPerformanceMetrics(sqlQuery, collectionName, requestOptions, totalRUs, startTime);

            return documents;
        }

        public async Task<List<T>> GetDocumentsAsync<T>(string sqlQuery, string collectionName, IRequestOptions requestOptions)
        {
            //Here we are using cosmosClient instead of documentClient as documentClient as not able to return Rus details
            //wit group query. Later may be we will use only one client.

            var container = _cosmosClient.GetContainer(_dataBaseName, collectionName);
            var queryDefinition = new MAC.QueryDefinition(sqlQuery);
            var queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);
            var result = new List<T>();
            double totalRUs = 0;
            var startTime = DateTime.UtcNow;

            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                totalRUs += response.RequestCharge;
                result.AddRange(response.ToList());
            }

           // AddPerformanceMetrics(sqlQuery, collectionName, requestOptions, totalRUs, startTime);

            return result;
        }

        public async Task<TEntity> AddAsync<TEntity>(TEntity entity, IRequestOptions requestOptions)
        {
            try
            {
                var entityConfigurationOptions = GetEntityConfigurationOptions(typeof(TEntity));
                //var referentialIntegrityHelper = new ReferentialIntegrityHelper<TEntity>(this, requestOptions.ServiceContext);
                //await referentialIntegrityHelper.ValidateForeignKeysOnAddAsync(entity, entityConfigurationOptions);

                var startTime = DateTime.UtcNow;
                var doc = await _documentClient.CreateDocumentAsync(CreateCollectionLink<TEntity>(entityConfigurationOptions), entity, new Microsoft.Azure.Documents.Client.RequestOptions(), false);
                //AddPerformanceMetrics("Added Entity", entityConfigurationOptions.CollectionName, requestOptions, doc.RequestCharge, startTime);

                return ConvertToEntity<TEntity>(doc);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TEntity> UpSertAsync<TEntity>(TEntity entity, IRequestOptions requestOptions)
        {
            var eTag = ((IDomainEntity)entity).ConcurrencyToken;
            ((IDomainEntity)entity).ConcurrencyToken = null;

            AccessCondition accessCondition = null;

            if (!string.IsNullOrWhiteSpace(eTag))
            {
                accessCondition = new AccessCondition() { Condition = eTag, Type = AccessConditionType.IfMatch };
            }

            var entityConfigurationOptions = GetEntityConfigurationOptions(typeof(TEntity));
            ResourceResponse<Document> doc;
            //var originalEntityCopy = Clone(entity);
            try
            {
                //var referentialIntegrityHelper = new ReferentialIntegrityHelper<TEntity>(this, requestOptions.ServiceContext);
                //await referentialIntegrityHelper.ValidateForeignKeysOnAddAsync(entity, entityConfigurationOptions);

                var startTime = DateTime.UtcNow;
                doc = await _documentClient.UpsertDocumentAsync(CreateCollectionLink<TEntity>(entityConfigurationOptions), entity,
                                new Microsoft.Azure.Documents.Client.RequestOptions() { AccessCondition = accessCondition }, false);

               // AddPerformanceMetrics("Upserted Entity", entityConfigurationOptions.CollectionName, requestOptions, doc.RequestCharge, startTime);
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
                {
                    throw new Exception($"ConcurrencyToken {eTag} is obsolete. Please refetch");
                }

                _logger.LogError(ex, "Error during upsert. Entity: {Entity}, requestOptions: {Options}", entity, requestOptions);
                throw;
            }

            return ConvertToEntity<TEntity>(doc);
            // return originalEntityCopy;
        }

        public async Task DeleteAsync<TEntity>(string id, string partitionKey, IRequestOptions requestOptions)
        {
            var entityConfigurationOptions = GetEntityConfigurationOptions(typeof(TEntity));

            //ReferentialIntegrityHelper<TEntity> referentialIntegrityHelper = new ReferentialIntegrityHelper<TEntity>(this, requestOptions.ServiceContext);

            //await referentialIntegrityHelper.ValidateForeignKeysOnDeleteAsync(id, entityConfigurationOptions);

            var startTime = DateTime.UtcNow;
            var doc = await _documentClient.DeleteDocumentAsync(
                        $"{CreateCollectionLink<TEntity>(entityConfigurationOptions)}/docs/{id}",
                         new Microsoft.Azure.Documents.Client.RequestOptions() { PartitionKey = new PartitionKey(partitionKey) });

            //AddPerformanceMetrics("Deleted Entity", entityConfigurationOptions.CollectionName, requestOptions, doc.RequestCharge, startTime);
        }

        public EntityConfigurationOptions GetEntityConfigurationOptions(Type entityType)
        {
            return _entityConfigLookUp[entityType.Name];
        }

        public Dictionary<string, EntityConfigurationOptions> GetEntityConfigurationForAll()
        {
            return _entityConfigLookUp;
        }

        //private void AddPerformanceMetrics(string sqlQuery, string collectionName, IRequestOptions requestOptions, double totalRUs, DateTime startTime)
        //{
        //    if (requestOptions?.ServiceContext != null)
        //    {
        //        var metrics = new List<KeyValuePair<string, double>>();
        //        metrics.Add(new KeyValuePair<string, double>("RUs Consumption", totalRUs));
        //        var executionTime = Math.Round((DateTime.UtcNow - startTime).TotalMilliseconds, 2);
        //        metrics.Add(new KeyValuePair<string, double>("Query Execution Time (in ms)", Math.Round((DateTime.UtcNow - startTime).TotalMilliseconds, 2)));
        //        requestOptions.ServiceContext.AddPerformanceMetric("Cosmos", $"Query:{sqlQuery}  Collection:{collectionName}", metrics);

        //        _logger.LogInformation("Query:{sqlQuery}|Collection:{collectionName} " +
        //            "| RUs Consumption:{totalRUs}|Execution Time:{executionTime} [ServiceContext: {ServiceContext}]", sqlQuery, collectionName, totalRUs, executionTime, requestOptions.ServiceContext.AsString);
        //    }
        //}

        private TEntity Clone<TEntity>(TEntity entity)
        {
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(entity), deserializeSettings);
        }

        private void SetEntityConfigurationOptions(IEnumerable<IEntityTypeConfiguration> entityTypeConfigurations)
        {
            foreach (var typeConfig in entityTypeConfigurations)
            {
                dynamic typeConfiguration = Activator.CreateInstance(typeConfig.GetType());
                Type generic = typeof(EntityTypeBuilder<>);
                var entityType = typeConfig.GetType().GetInterfaces()[0].GetGenericArguments()[0];

                Type constructed = generic.MakeGenericType(entityType);
                dynamic builder = Activator.CreateInstance(constructed);

                typeConfiguration.Configure(builder);
                EntityConfigurationOptions entityConfigurationOptions = builder.GetConfiguration();
                _entityConfigLookUp[typeConfig.GetType().GetInterfaces()[0].GetGenericArguments().First().Name] = entityConfigurationOptions;
            }
        }

        private string CreateCollectionLink<TEntity>(EntityConfigurationOptions entityConfigurationOptions)
        {
            return CreateCollectionLink(entityConfigurationOptions.CollectionName);
        }

        private string CreateCollectionLink(string collectionName)
        {
            return string.Format(
               "/dbs/{0}/colls/{1}",
              _dataBaseName,
              collectionName);
        }

        private TEntity ConvertToEntity<TEntity>(Document doc)
        {
            return JsonConvert.DeserializeObject<TEntity>(doc.ToString());
        }
    }
}
