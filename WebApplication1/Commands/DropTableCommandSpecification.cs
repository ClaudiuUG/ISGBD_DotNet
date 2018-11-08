using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class DropTableCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        private string _insertTableRegex = "^" + RegexStrings.DropCommandRegex + RegexStrings.TableReservedWordRegex + RegexStrings.IdentifierRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _insertTableRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var commandRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = commandRegex.Matches(command);
            var tableName = componentMatch[TableNamePosition].Value;

            parsedCommand = new DropTableCommand(tableName);
            return true;
        }
    }
}