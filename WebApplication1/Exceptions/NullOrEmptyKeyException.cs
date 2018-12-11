using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class NullOrEmptyKeyException : Exception
    {
        public NullOrEmptyKeyException(string key) : base("Null or empty key: " + key)
        {
        }
    }
}