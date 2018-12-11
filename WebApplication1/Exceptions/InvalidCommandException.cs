using System;

namespace KeyValueDatabaseApi.Exceptions
{
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException() { }
        public InvalidCommandException(string message) : base(message) { }
    }
}