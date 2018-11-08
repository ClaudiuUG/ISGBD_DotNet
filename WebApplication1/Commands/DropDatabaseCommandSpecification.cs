using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class DropDatabaseCommandSpecification : ICommandSpecification
    {
        private const int DatabaseNamePosition = 2;

        private string _createDataBaseRegex = "^" + RegexStrings.DropCommandRegex + RegexStrings.DatabaseReservedWordRegex + RegexStrings.IdentifierRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _createDataBaseRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }
 
            var commandRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = commandRegex.Matches(command);
            var databaseName = componentMatch[DatabaseNamePosition].Value;

            parsedCommand = new DropDatabaseCommand(databaseName);
            return true;
        }
    }
}