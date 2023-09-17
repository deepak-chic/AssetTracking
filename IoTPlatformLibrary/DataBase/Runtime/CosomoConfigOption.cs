using System;

namespace IoTPlatformLibrary.DataBase.Runtime
{
    public class CosomoConfigOption
    {
        public const string ConfigName = "CosmoConfig";
        public Uri AccountEndpoint { get; set; }
        public string AccountKey { get; set; }
        public int RequestUnits { get; set; }
        public string DataBase { get; set; }
    }
}
