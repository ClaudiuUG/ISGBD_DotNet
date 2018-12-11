using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class TableAlreadyExistsException : Exception
    {
        public TableAlreadyExistsException(string tableName)
            : base($"Table {tableName} already exists.")
        {
        }
    }
}