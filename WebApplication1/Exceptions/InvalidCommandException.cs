namespace KeyValueDatabaseApi.Exceptions
{
    public class InvalidCommandException : System.Exception
    {
        public InvalidCommandException() { }
        public InvalidCommandException(string message) : base(message) { }
    }
}