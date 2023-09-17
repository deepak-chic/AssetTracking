using IoTPlatformLibrary.DataBase.Abstract;
using IoTPlatformLibrary.DataBase.Runtime;
using Newtonsoft.Json;

namespace IoTPlatformLibrary.DataBase
{
    public class ReferentialIntegrityHelper<TEntity>
    {
        IDocumentHelper _documentHelper;
        IServiceContext _serviceContext;

        public ReferentialIntegrityHelper(IDocumentHelper documentHelper, IServiceContext serviceContext)
        {
            _documentHelper = documentHelper;
            _serviceContext = serviceContext;
        }

        //public async Task ValidateForeignKeysOnAddAsync(TEntity entity, EntityConfigurationOptions entityConfigurationOptions)
        //{
        //    foreach (var foreignKey in entityConfigurationOptions.ForeignKeys)
        //    {
        //        var foreignKeyValue = GetPropValue(entity, foreignKey.Key).ToString();

        //        //TODO:partition key inclusion in query
        //        var result = await _documentHelper.GetDocumentsAsync($"SELECT * FROM c where c.id=\"{foreignKeyValue}\"", foreignKey.Collection, new RequestOptions(_serviceContext));

        //        if (!result.Any())
        //        {
        //            throw new ForeignKeyViolationException(foreignKey.Collection, foreignKey.Key, foreignKeyValue);
        //        }

        //        SetToNull(entity, foreignKey.HoldingProperty);
        //    }
        //}

        //public async Task ValidateForeignKeysOnDeleteAsync(object id, EntityConfigurationOptions entityConfigurationOptions)
        //{
        //    var dic = _documentHelper.GetEntityConfigurationForAll();

        //    foreach (var key in dic.Keys)
        //    {
        //        var foreignKey = dic[key].ForeignKeys.FirstOrDefault(x => x.Collection.Equals(entityConfigurationOptions.CollectionName, StringComparison.OrdinalIgnoreCase));

        //        if (foreignKey != null && !string.IsNullOrWhiteSpace(foreignKey.Key))
        //        {
        //            //TODO:partition key inclusion in query
        //            var result = await _documentHelper.GetDocumentsAsync($"SELECT * FROM c where c.{foreignKey.Key} =\"{id.ToString()}\"", dic[key].CollectionName, new RequestOptions(_serviceContext));

        //            if (result.Any())
        //            {
        //                throw new ReferentialIntegrityException(dic[key].CollectionName, foreignKey.Key, id.ToString());
        //            }
        //        }
        //    }
        //}

        //public async Task PopulateReferentialObjectsAsync(IEnumerable<object> entities, EntityConfigurationOptions entityConfigurationOptions)
        //{
        //    foreach (var foreignKey in entityConfigurationOptions.ForeignKeys)
        //    {
        //        List<string> ids = new List<string>();

        //        if (_serviceContext != null &&
        //            _serviceContext.QueryOptimization.GetIncludedEntities().FirstOrDefault(x => x == foreignKey.HoldingProperty) == null)
        //        {
        //            continue;
        //        }

        //        List<dynamic> result = await GetAllReferencedEntities(entities, foreignKey, ids);

        //        if (result.Any())
        //        {
        //            var refEntities = FindSetValue(entities, result, foreignKey);
        //            await PopulateReferentialObjectOfReferentialObjects(refEntities);
        //        }
        //    }
        //}

        //private async Task PopulateReferentialObjectOfReferentialObjects(IEnumerable<object> objs)
        //{
        //    EntityConfigurationOptions config = _documentHelper.GetEntityConfigurationOptions(objs.First().GetType());
        //    Type generic = typeof(ReferentialIntegrityHelper<>);
        //    Type constructed = generic.MakeGenericType(objs.First().GetType());
        //    dynamic referentialIntegrityHelper = Activator.CreateInstance(constructed, new object[] { _documentHelper, _serviceContext });
        //    await referentialIntegrityHelper.PopulateReferentialObjectsAsync(objs, config);
        //}

        private IEnumerable<object> FindSetValue(IEnumerable<object> entities, IEnumerable<dynamic> referencedDocuments, ForeignKey foreignKey)
        {
            List<object> list = new List<object>();

            foreach (var entity in entities)
            {
                var value = GetPropValue(entity, foreignKey.Key).ToString();

                var referencedDocument = referencedDocuments.FirstOrDefault(x => x.id == value);

                if (referencedDocument != null)
                {
                    var obj = SetValue(entity, foreignKey.HoldingProperty, referencedDocument.ToString());

                    list.Add(obj);
                }
            }

            return list;
        }

        private async Task<List<dynamic>> GetAllReferencedEntities(IEnumerable<object> entities, ForeignKey foreignKey, List<string> ids)
        {
            foreach (var entity in entities)
            {
                ids.Add(GetPropValue(entity, foreignKey.Key).ToString());
            }

            var entityIds = string.Join(",", ids.Select(x => string.Format("'{0}'", x)).ToList());

            string partitionKeyCondition = $"STARTSWITH(c.PartitionKey,'{_serviceContext.TenantId}')";
            string query = $"SELECT * FROM c where c.id in ({ entityIds}) and {partitionKeyCondition}";
            //TODO:partition key inclusion in query
            var result = await _documentHelper.GetDocumentsAsync(query, foreignKey.Collection, new RequestOptions(_serviceContext));
            return result;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        private static void SetToNull(object src, string propName)
        {
            src.GetType().GetProperty(propName).SetValue(src, null);
        }

        private object SetValue(object src, string propName, string value)
        {
            var prop = src.GetType().GetProperty(propName);
            var type = prop.PropertyType;
            var obj = JsonConvert.DeserializeObject(value, type);
            src.GetType().GetProperty(propName).SetValue(src, obj);

            return obj;
        }
    }
}
