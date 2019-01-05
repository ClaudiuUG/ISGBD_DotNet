using System;

namespace KeyValueDatabaseApi.Context
{
    internal static class PathHelper
    {
        private static readonly string DatabasesPath = $@"C:\Users\Cristiana\Source\Repos\ISGBD_DotNet\WebApplication1\DatabaseStorage";
        //public static readonly string DatabasesPath = $@"C:\Users\Claudiu\source\repos\WebApplication1\WebApplication1\DatabaseStorage";

        public static readonly string MetadataFilePath = $@"{DatabasesPath}\Metadata.json";

        public static string GetTablePath(string databaseName, string tableName)
        {
            return DatabasesPath + @"\" + databaseName + @"\" + tableName;
        }

        public static string GetIndexPath(string databaseName, string tableName, string indexName)
        {
            return DatabasesPath + @"\" + databaseName + @"\index\" + tableName + @"\" + indexName;
        }

        public static string GetReferencedTablePath(string databaseName, string tableName, string referencedTableName)
        {
            return DatabasesPath + @"\" + databaseName + @"\index\" + tableName + @"\" + referencedTableName;
        }
    }
}