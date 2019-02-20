using System;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectCommandSpecification : ICommandSpecification
    {
        // select * from tableName where id = 1
        // group by columnone having count|sum = 4
        private readonly string _selectRegex = "^" +
            RegexStrings.SelectReservedWordRegex +
            RegexStrings.SelectSubject +
            RegexStrings.FromReservedWordRegex +
            RegexStrings.IdentifierRegex;

        private readonly string _selectWhereRegex =
            "^" +
            RegexStrings.SelectReservedWordRegex +
            RegexStrings.SelectSubject +
            RegexStrings.FromReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.WhereReservedWordRegex +
            RegexStrings.IdentifierRegex +
            RegexStrings.EqualOperatorRegex +
            RegexStrings.RowEntryValueWithSpaces;

        private readonly string _groupByRegex =
            RegexStrings.GroupByReservedWordsRegex +
            RegexStrings.IdentifierRegex;


        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var containsHaving = Regex.Match(command, "having").Success;
            AggregatorFunction? aggregatorFunction = null;
            var groupByHavingComparer = string.Empty;
            int? groupByHavingValue = null;
            if (containsHaving)
            {
                if (Regex.Match(command, "count").Success)
                {
                    aggregatorFunction = AggregatorFunction.Count;
                }
                if (Regex.Match(command, "sum").Success)
                {
                    aggregatorFunction = AggregatorFunction.Sum;
                }

                var havingMatch = Regex.Match(command, "(<|<=|=|>=|>)\\s+([0-9]+)");
                groupByHavingComparer = havingMatch.Groups[1].Value;
                groupByHavingValue = int.Parse(havingMatch.Groups[2].Value);
            }

            var groupByMatch = Regex.Match(command, _groupByRegex);
            var hasGroupBy = groupByMatch.Success;

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
                if (!hasGroupBy)
                {
                    parsedCommand = new SelectCommand(tableName, columnName, keyToFind);
                    return true;
                }
            }

            if (hasGroupBy)
            {
                var groupByCondition = groupByMatch.Value;
                groupByCondition = groupByCondition.Remove(0, "group by".Length + 2);
                var groupByColumn = groupByCondition.TrimStart();
                if (containsHaving)
                {
                    parsedCommand = new SelectCommand(tableName, groupByColumn, groupByHavingValue,
                        groupByHavingComparer, aggregatorFunction.Value);
                    return true;
                }
                else
                {
                    parsedCommand = new SelectCommand(tableName)
                    {
                        GroupByColumn = groupByColumn
                    };
                    return true;
                }
            }

            parsedCommand = new SelectCommand(tableName);
            return true;
        }
    }
}