using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class InsertIntoCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        public string InsertIntoCommandRegex = "^" +
            RegexStrings.InsertIntoReservedWordsRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.ParameterListRegex +
            RegexStrings.ValuesReservedKeyword +
            RegexStrings.RowEntryValueListRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, InsertIntoCommandRegex);
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

            var rowValuesWithoutParenthesesValue = listWithParenthesesMatch.NextMatch().Value;
            var valuesWithoutParenthesesValue = Regex.Match(rowValuesWithoutParenthesesValue, RegexStrings.RowEntryValueListWithoutParenthesisRegex).Value;
            var rowEntryValues = Regex.Match(valuesWithoutParenthesesValue, RegexStrings.RowEntryValue);
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