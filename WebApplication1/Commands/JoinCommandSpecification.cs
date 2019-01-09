using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class JoinCommandSpecification : ICommandSpecification
    {
        private readonly string _joinRegex = 
            "^" +
            RegexStrings.SelectReservedWordRegex +
            RegexStrings.TableColumnsWithoutParenthesesRegex +
            RegexStrings.FromReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.JoinTypeRegex +
            RegexStrings.JoinReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.OnReservedWordRegex +
            RegexStrings.PrefixedColumnRegex +
            RegexStrings.EqualOperator +
            RegexStrings.PrefixedColumnRegex +
            "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var joinMatch = Regex.Match(command, _joinRegex);
            if (!joinMatch.Success)
            {
                parsedCommand = null;
                return false;
            }

            var indexOfFrom = command.IndexOf(RegexStrings.FromReservedWordWithoutSpacesRegex, StringComparison.Ordinal);
            var commandAfterSelect = command.Skip(RegexStrings.SelectReservedWordWithoutSpacesRegex.Length + 1);
            var columns = commandAfterSelect.Take(indexOfFrom).ToString().Replace(" ", string.Empty).Split(',').ToList();

            var commandAfterFrom = command.Remove(0, indexOfFrom + RegexStrings.FromReservedWordWithoutSpacesRegex.Length);
            var tableName = Regex.Match(commandAfterFrom, RegexStrings.IdentifierRegex).Value;

            var joinType = Regex.Match(command, RegexStrings.JoinTypeWithoutSpacesRegex).Value;

            var indexOfJoin = command.IndexOf(RegexStrings.JoinWithoutSpacesRegex, StringComparison.Ordinal);
            var commandAfterJoin = command.Remove(0, indexOfJoin + RegexStrings.JoinWithoutSpacesRegex.Length);

            var tableToJoin = Regex.Match(RegexStrings.IdentifierRegex, commandAfterJoin).Value;

            var indexOfOn = command.IndexOf(RegexStrings.OnReservedWordWithoutSpacesRegex, StringComparison.Ordinal);
            var commandAfterOn = command.Remove(0, indexOfOn + RegexStrings.OnReservedWordWithoutSpacesRegex.Length);

            var prefixedColumnsMatch = Regex.Match(commandAfterOn, RegexStrings.PrefixedColumnWithoutSpacesRegex);
            var firstPrefixedColumn = GetPrefixedColumnFromString(prefixedColumnsMatch.Value);
            prefixedColumnsMatch.NextMatch();
            var secondPrefixedColumn = GetPrefixedColumnFromString(prefixedColumnsMatch.Value);
            
            parsedCommand = new JoinCommand(tableName, tableToJoin, firstPrefixedColumn.ColumnName, secondPrefixedColumn.ColumnName, columns, joinType.Equals("loop") ? JoinType.IndexNestedLoopJoin : JoinType.HashJoin);
            return true;
        }

        private PrefixedColumn GetPrefixedColumnFromString(string value)
        {
            var values = value.Split('.');
            return new PrefixedColumn(values[0], values[1]);
        }

        public class PrefixedColumn
        {
            public PrefixedColumn(string tableName, string columnName)
            {
                TableName = tableName;
                ColumnName = columnName;
            }

            public string TableName { get; }
            public string ColumnName { get; }
        }
    }
}