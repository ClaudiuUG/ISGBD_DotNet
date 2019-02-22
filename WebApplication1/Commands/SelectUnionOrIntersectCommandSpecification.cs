using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectUnionOrIntersectCommandSpecification : ICommandSpecification
    {
        private readonly string _selectUnionRegex =
            "^"
            + RegexStrings.SelectReservedWordRegex
            + $@"(({RegexStrings.IdentifierRegex})|(\*))"
            + RegexStrings.FromReservedWordRegex
            + RegexStrings.IdentifierRegex
            + @"\s+(union)|(intersect)\s+"
            + RegexStrings.SelectReservedWordRegex
            + $@"(({RegexStrings.IdentifierRegex})|(\*))"
            + RegexStrings.FromReservedWordRegex
            + RegexStrings.IdentifierRegex;

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            if (!Regex.Match(command, _selectUnionRegex).Success)
            {
                parsedCommand = null;
                return false;
            }

            var tableNameMatch = Regex.Match(command, $@"(from\s*)({RegexStrings.IdentifierRegex})(\s|$)");
            var firstTableName = tableNameMatch.Groups[2].Value;
            tableNameMatch = tableNameMatch.NextMatch();
            var secondTableName = tableNameMatch.Groups[2].Value;

            var setFunction = Regex.Match(command, "union").Success ? SetFunction.Union : SetFunction.Intersect;

            parsedCommand = new SelectUnionOrIntersectCommand(firstTableName, secondTableName, setFunction);
            return true;
        }
    }
}