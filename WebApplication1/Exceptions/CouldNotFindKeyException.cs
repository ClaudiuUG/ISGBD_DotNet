using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class CouldNotFindKeyException : Exception
    {
        public CouldNotFindKeyException(string key) : base("Could not find key: " + key)
        {
        }
    }
}