namespace KeyValueDatabaseApi.Commands
{
    public interface ICommandSpecification
    {
        bool TryParse(string command, out ICommand parsedCommand);
    }
}