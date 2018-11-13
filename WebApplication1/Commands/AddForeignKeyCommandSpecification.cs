using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class AddForeignKeyCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        private readonly string _addForeignKeyRegex =
            "^" +
            RegexStrings.AlterTableReservedWordsRegex +
            RegexStrings.IdentifierRegex + // table name
            RegexStrings.AddForeignKeyReservedWordsRegex +
            RegexStrings.ParameterListRegex + // column list for the new foreign key
            RegexStrings.ReferencesReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.RowEntryValueListRegex + 
            "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _addForeignKeyRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var wordFindingRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = wordFindingRegex.Matches(command);
            var tableName = componentMatch[TableNamePosition].Value;

            var listWithParenthesesMatch = Regex.Match(command, RegexStrings.RowEntryValueListRegex);
            var listWithParenthesesValue = listWithParenthesesMatch.Value;
            var tableColumnsWithoutParenthesesValue = Regex.Match(listWithParenthesesValue, RegexStrings.ParameterListWithoutParenthesesRegex).Value;
            var tableColumns = Regex.Match(tableColumnsWithoutParenthesesValue, RegexStrings.IdentifierRegex);
            var columnNames = BuildListFromMatchValues(tableColumns);

            // table name and column names acquired, the referenced table and referenced columns must be parsed now

            parsedCommand = null;
            return false;
        }

        public List<string> BuildListFromMatchValues(Match match)
        {
            var values = new List<string>();
            while (match.Success)
            {
                var columnName = match.Value;
                values.Add(columnName);
                match = match.NextMatch();
            }
            return values;
        }
    }
}