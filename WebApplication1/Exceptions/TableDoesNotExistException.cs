using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class TableDoesNotExistException : Exception
    {
        public TableDoesNotExistException(string database, string table)
            : base($"Could not find table {table} in current database {database}")
        {
        }
    }
}