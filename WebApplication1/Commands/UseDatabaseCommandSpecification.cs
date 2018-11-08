using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class UseDatabaseCommandSpecification : ICommandSpecification
    {
        private const int DatabaseNamePosition = 2;

        private string _useDataBaseRegex = "^" + RegexStrings.UseCommandRegex + RegexStrings.DatabaseReservedWordRegex + RegexStrings.IdentifierRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _useDataBaseRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var commandRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = commandRegex.Matches(command);
            var databaseName = componentMatch[DatabaseNamePosition].Value;

            parsedCommand = new UseDatabaseCommand(databaseName);
            return true;
        }
    }
}