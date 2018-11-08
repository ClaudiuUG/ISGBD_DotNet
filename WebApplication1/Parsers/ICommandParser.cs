using KeyValueDatabaseApi.Commands;

namespace KeyValueDatabaseApi.Parsers
{
    public interface ICommandParser
    {
        bool TryParse(string command, out ICommand parsedCommand);
    }
}