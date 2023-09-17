using System.Linq.Expressions;
using IoTPlatformLibrary.Extensions;

namespace IoTPlatformLibrary.DataBase.Runtime
{
    public class EntityTypeBuilder<TEntity>
    {
        EntityConfigurationOptions _entityConfigurationOptions;

        public EntityTypeBuilder()
        {
            _entityConfigurationOptions = new EntityConfigurationOptions();
        }
        public EntityTypeBuilder<TEntity> ToCollection(string collectionName)
        {
            _entityConfigurationOptions.CollectionName = collectionName;
            return this;
        }

        public EntityTypeBuilder<TEntity> CodePrefix(string codePrefix)
        {
            _entityConfigurationOptions.CodePrefix = codePrefix;
            return this;
        }

        public EntityTypeBuilder<TEntity> HasPartitionKey(Expression<Func<TEntity, object>> keyExpression)
        {
            _entityConfigurationOptions.PartitionKeys.Add(keyExpression.GetMemberName());

            return this;
        }

        public EntityTypeBuilder<TEntity> HasForeignKey(Expression<Func<TEntity, object>> keyExpression)
        {
            string foreignkey = keyExpression.GetMemberName();
            _entityConfigurationOptions.ForeignKeys.Add(new ForeignKey() { Key = foreignkey });

            return this;
        }

        public EntityTypeBuilder<TEntity> ForeignKeyCollectionName(string collectionName)
        {
            var foreignkey = _entityConfigurationOptions.ForeignKeys.LastOrDefault();
            foreignkey.Collection = collectionName;

            return this;
        }

        public EntityTypeBuilder<TEntity> MapFKTo(Expression<Func<TEntity, object>> keyExpression)
        {
            string propertyName = keyExpression.GetMemberName();
            var foreignkey = _entityConfigurationOptions.ForeignKeys.LastOrDefault();
            foreignkey.HoldingProperty = propertyName;

            return this;
        }

        public EntityConfigurationOptions GetConfiguration()
        {
            return _entityConfigurationOptions;
        }
    }
}
