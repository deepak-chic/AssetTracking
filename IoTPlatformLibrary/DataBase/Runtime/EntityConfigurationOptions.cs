namespace IoTPlatformLibrary.DataBase.Runtime
{
    public class EntityConfigurationOptions
    {
        public EntityConfigurationOptions()
        {
            ForeignKeys = new List<ForeignKey>();
            PartitionKeys = new List<string>();
        }

        public List<string> PartitionKeys { get; set; }

        public string CollectionName { get; set; }

        public List<ForeignKey> ForeignKeys { get; set; }
        public string CodePrefix { get; set; }

    }

    public class ForeignKey
    {
        public string Key { get; set; }
        public string Collection { get; set; }
        public string HoldingProperty { get; set; }
    }
}
