using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateIndexCommandSpecification : ICommandSpecification
    {
        private const int IndexNamePosition = 2;
        private const int TableNamePosition = 4;

        private string _createIndexRegex = "^" + 
            RegexStrings.CreateCommandRegex + 
            RegexStrings.IndexReservedWordRegex + 
            RegexStrings.IdentifierRegex + 
            RegexStrings.OnReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.ParameterListRegex; 

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _createIndexRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var wordFindingRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = wordFindingRegex.Matches(command);
            var indexName = componentMatch[IndexNamePosition].Value;
            var tableName = componentMatch[TableNamePosition].Value;

            var tableColumnsWithParanthesis = Regex.Match(command, RegexStrings.ParameterListRegex).Value;
            var tableColumnsWithoutParanthesis = Regex.Match(tableColumnsWithParanthesis, RegexStrings.ParameterListWithoutParanthesisRegex).Value;
            var tableColumns = Regex.Match(tableColumnsWithoutParanthesis, RegexStrings.IdentifierRegex);

            var columnNames = new List<string>();
            while (tableColumns.Success)
            {
                var columnName = tableColumns.Value;
                columnNames.Add(columnName);
                tableColumns = tableColumns.NextMatch();
            }

            parsedCommand = new CreateIndexCommand(indexName, tableName, columnNames);
            return true;
        }
    }
}