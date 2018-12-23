using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class KeyAlreadyStoredException : Exception
    {
        public KeyAlreadyStoredException(string tablePath, string key)
            : base($"Table {tablePath} already has an entry stored for key: {key}")
        {
        }
    }
}