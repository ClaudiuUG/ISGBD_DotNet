using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class IndexingColumnDoesNotExist : Exception
    {
        public IndexingColumnDoesNotExist(string databaseName, string tableName, string indexColumnName) 
            : base($"Table {tableName} from database {databaseName} does not contain column {indexColumnName}")
        {
        }
    }
}