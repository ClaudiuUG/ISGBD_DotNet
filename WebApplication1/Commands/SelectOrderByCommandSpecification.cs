using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectOrderByCommandSpecification : ICommandSpecification
    {
        private readonly string _selectUnionRegex =
            "^"
            + RegexStrings.SelectReservedWordRegex
            + ".*"
            + RegexStrings.FromReservedWordRegex
            + RegexStrings.IdentifierRegex
            + @"\s+order\s+by\s+"
            + RegexStrings.IdentifierRegex;

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            if (!Regex.Match(command, _selectUnionRegex).Success)
            {
                parsedCommand = null;
                return false;
            }

            var tableNameMatch = Regex.Match(command, $@"(from\s*)({RegexStrings.IdentifierRegex})");
            var tableName = tableNameMatch.Groups[2].Value;

            var columnNameMatch = Regex.Match(command, $@"(by\s*)({RegexStrings.IdentifierRegex})");
            var columnName = columnNameMatch.Groups[2].Value;

            parsedCommand = new SelectOrderByCommand(tableName, columnName);
            return true;
        }
    }
}