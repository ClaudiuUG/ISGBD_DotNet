using System.Linq;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectOuterJoinCommandSpecification : ICommandSpecification
    {
        private readonly string _selectOuterJoinRegex = "^"
                                                        + RegexStrings.SelectReservedWordRegex
                                                        + @".+"
                                                        + RegexStrings.FromReservedWordRegex
                                                        + RegexStrings.IdentifierRegex
                                                        + @"\s*((right)|(left))\s*"
                                                        + @"(outer\s+join)|(outer\s+join)\s*"
                                                        + RegexStrings.IdentifierRegex
                                                        + RegexStrings.OnReservedWordRegex
                                                        + RegexStrings.IdentifierRegex + "."
                                                        + RegexStrings.IdentifierRegex
                                                        + @"\s+" + RegexStrings.EqualOperator + @"\s+"
                                                        + RegexStrings.IdentifierRegex + "." +
                                                        RegexStrings.IdentifierRegex;

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            if (!Regex.Match(command, _selectOuterJoinRegex).Success)
            {
                parsedCommand = null;
                return false;
            }

            var columnNamesMatch = Regex.Match(command, @"(select)\s+(.+)\s+(from)");
            var columnNames = columnNamesMatch.Groups[2].Value.Split(',').ToList().Select(rawColumnName => rawColumnName.Trim(' '));

            var firstTableMatch = Regex.Match(command, @"(from\s*)(.+)(\s*right|left)");
            var firstTableName = firstTableMatch.Groups[2].Value;

            var secondTableMatch = Regex.Match(command, @"(join\s*)(.+)(\s*on)");
            var secondTableName = secondTableMatch.Groups[2].Value;

            var joinColumnsMatch = Regex.Match(command, @"(\.)([^\s]*)");
            var firstJoinColumnName = joinColumnsMatch.Groups[2].Value;
            joinColumnsMatch = joinColumnsMatch.NextMatch();
            var secondJoinColumnName = joinColumnsMatch.Groups[2].Value;

            var joinDirection = Regex.Match(command, "left").Success ? JoinDirection.Left : JoinDirection.Right;

            parsedCommand = new SelectOuterJoinCommand(firstTableName, secondTableName, firstJoinColumnName, secondJoinColumnName, columnNames, joinDirection);
            return true;
        }
    }
}