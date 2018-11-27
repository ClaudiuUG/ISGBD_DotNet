using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class AddForeignKeyCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;
        private const int ReferencedTablePositionFromReferences = 1;

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
            var referencesIndex = command.IndexOf(RegexStrings.ReferencesWithoutSpacesRegex, StringComparison.Ordinal);
            var commandFromReferences = command.Remove(0, referencesIndex);
            var componentMatchFromReferences = wordFindingRegex.Matches(commandFromReferences);
            var referencedTableName = componentMatchFromReferences[ReferencedTablePositionFromReferences].Value;

            var parameterLists = GetParameterListsFromCommand(command);
            var tableColumns = parameterLists.First();
            var referencedTableColumns = parameterLists.ElementAt(1);

            parsedCommand = new AddForeignKeyCommand(tableName, tableColumns, referencedTableName, referencedTableColumns);
            return true;
        }

        public List<List<string>> GetParameterListsFromCommand(string command)
        {
            var parameterLists = new List<List<string>>();
            var listWithParenthesesMatch = Regex.Match(command, RegexStrings.RowEntryValueListRegex);
            while (listWithParenthesesMatch.Success)
            {
                var listWithParenthesesValue = listWithParenthesesMatch.Value;
                var tableColumnsWithoutParenthesesValue = Regex.Match(listWithParenthesesValue, RegexStrings.ParameterListWithoutParenthesesRegex).Value;
                var tableColumns = Regex.Match(tableColumnsWithoutParenthesesValue, RegexStrings.IdentifierRegex);
                var columnNames = BuildListFromMatchValues(tableColumns);
                parameterLists.Add(columnNames);
                listWithParenthesesMatch = listWithParenthesesMatch.NextMatch();
            }

            return parameterLists;
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