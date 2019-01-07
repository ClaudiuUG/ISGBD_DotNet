using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectCommandSpecification : ICommandSpecification
    {
        private readonly string _selectRegex = "^" +
            RegexStrings.SelectReservedWordRegex +
            RegexStrings.SelectSubject +
            RegexStrings.FromReservedWordRegex +
            RegexStrings.IdentifierRegex +
            "$";
        private readonly string _selectWhereRegex =
            "^" +
            RegexStrings.SelectReservedWordRegex +
            RegexStrings.SelectSubject +
            RegexStrings.FromReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.WhereReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.EqualOperatorRegex +
            RegexStrings.RowEntryValueWithSpaces +
            "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var hasWhereClause = true;
            var selectWhereMatch = Regex.Match(command, _selectWhereRegex);
            if (!selectWhereMatch.Success)
            {
                var selectMatch = Regex.Match(command, _selectRegex);
                if (!selectMatch.Success)
                {
                    parsedCommand = null;
                    return false;
                }

                hasWhereClause = false;
            }

            // update the parsing and regex if we ever want to support select with projection
            var indexOfFrom = command.IndexOf(RegexStrings.FromReservedWordWithoutSpacesRegex, StringComparison.Ordinal);
            var commandAfterFrom = command.Remove(0, indexOfFrom + RegexStrings.FromReservedWordWithoutSpacesRegex.Length);
            var tableName = Regex.Match(commandAfterFrom, RegexStrings.IdentifierRegex).Value;

            if (hasWhereClause)
            {
                var indexOfWhere = command.IndexOf(RegexStrings.WhereReservedWordWithoutSpacesRegex, StringComparison.Ordinal);
                var commandAfterWhere = command.Remove(0, indexOfWhere + RegexStrings.WhereReservedWordWithoutSpacesRegex.Length);
                var columnName = Regex.Match(commandAfterWhere, RegexStrings.IdentifierRegex).Value;
                var indexOfEqual = command.IndexOf(RegexStrings.EqualOperator, StringComparison.Ordinal);
                var commandFromEquals = command.Remove(0, indexOfEqual);
                var keyToFind = Regex.Match(commandFromEquals, RegexStrings.RowEntryValue).Value;
                parsedCommand = new SelectCommand(tableName, columnName, keyToFind);
                return true;
            }

            parsedCommand = new SelectCommand(tableName);
            return true;
        }
    }
}