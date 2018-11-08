using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class InsertIntoCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        public string _insertIntoCommandRegex = "^" +
            RegexStrings.InsertIntoCommandRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.ParameterListRegex +
            RegexStrings.ValuesReservedKeyword +
            RegexStrings.RowEntryValueListRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _insertIntoCommandRegex);
            if (!match.Success) 
            {
                parsedCommand = null;
                return false;
            }

            var wordFindingRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = wordFindingRegex.Matches(command);
            var tableName = componentMatch[TableNamePosition].Value;

            var listWithParanthesisMatch = Regex.Match(command, RegexStrings.RowEntryValueListRegex);
            var listWithParanthesisValue = listWithParanthesisMatch.Value;
            var tableColumnsWithoutParanthesisValue = Regex.Match(listWithParanthesisValue, RegexStrings.ParameterListWithoutParanthesisRegex).Value;
            var tableColumns = Regex.Match(tableColumnsWithoutParanthesisValue, RegexStrings.IdentifierRegex);
            var columnNames = BuildListFromMatchValues(tableColumns);

            var rowValuesWithoutParanthesisValue = listWithParanthesisMatch.NextMatch().Value;
            var valuesWithoutParanthesisValue = Regex.Match(rowValuesWithoutParanthesisValue, RegexStrings.RowEntryValueListWithoutParanthesisRegex).Value;
            var rowEntryValues = Regex.Match(valuesWithoutParanthesisValue, RegexStrings.RowEntryValue);
            var values = BuildListFromMatchValues(rowEntryValues);

            if (columnNames.Count != values.Count)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }

            parsedCommand = new InsertIntoCommand(tableName, columnNames, values);
            return true;
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