using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class IndexAlreadyExistsException : Exception
    {
        public IndexAlreadyExistsException(string databaseName, string tableName, string indexPath) : base($"Table {tableName} from {databaseName} already contains index {indexPath}")
        {
        }
    }
}