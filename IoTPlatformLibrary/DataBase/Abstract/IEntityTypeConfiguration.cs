using IoTPlatformLibrary.DataBase.Runtime;

namespace IoTPlatformLibrary.DataBase.Abstract
{
    public interface IEntityTypeConfiguration
    {

    }

    public interface IEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration where TEntity : class
    {
        void Configure(EntityTypeBuilder<TEntity> builder);
    }
}
