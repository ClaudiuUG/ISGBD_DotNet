namespace KeyValueDatabaseApi.Http.Requests
{
    public class ExecuteCommandRequest
    {
        public ExecuteCommandRequest(string command) 
        {
            Command = command;
        }

        public string Command { get; set; }
    }
}