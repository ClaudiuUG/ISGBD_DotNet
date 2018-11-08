using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;
using KeyValueDatabaseApi.Models;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateTableCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        private string _createTableRegex = "^" + RegexStrings.CreateCommandRegex + RegexStrings.TableReservedWordRegex + RegexStrings.IdentifierRegex + RegexStrings.TableColumnsRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, _createTableRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var wordFindingRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = wordFindingRegex.Matches(command);
            var tableName = componentMatch[TableNamePosition].Value;

            var tableColumnsMatch = Regex.Match(command, RegexStrings.TableColumnsWithoutParanthesisRegex);
            var tableColumnsNameAndType = tableColumnsMatch.Value;
            var identifierRegex = RegexStrings.IdentifierRegex + "|" + RegexStrings.ColumnTypeRegex;
            var tableColumnTypesAndNames = Regex.Match(tableColumnsNameAndType, identifierRegex);
            
            var attributes = new List<AttributeModel>();
            while (tableColumnTypesAndNames.Success)
            {
                var columnName = tableColumnTypesAndNames.Value.Replace(" ", string.Empty);
                tableColumnTypesAndNames = tableColumnTypesAndNames.NextMatch();
                var columnType = tableColumnTypesAndNames.Value.Replace(" ", string.Empty);
                tableColumnTypesAndNames = tableColumnTypesAndNames.NextMatch();
                attributes.Add(new AttributeModel(columnName, columnType));
            }

            parsedCommand = new CreateTableCommand(tableName, attributes);
            return true;
        }
    }
}