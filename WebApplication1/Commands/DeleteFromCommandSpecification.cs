﻿using System;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class DeleteFromCommandSpecification : ICommandSpecification
    {
        private const int TableNamePosition = 2;

        public string DeleteFromCommandRegex =
            @"^\s*delete\s*from\s*[A-Za-z0-9][A-Za-z0-9]*\s*where\s*\s*key\s*=\s*[A-Za-z0-9]+$";
                                               // "^" +
                                               //RegexStrings.DeleteFromReservedWordsRegex +
                                               //RegexStrings.IdentifierRegex +
                                               //RegexStrings.WhereReservedWordRegex +
                                               //RegexStrings.KeyEqualsValueRegex + "$";

        public bool TryParse(string command, out ICommand parsedCommand)
        {
            var match = Regex.Match(command, DeleteFromCommandRegex);
            if (!match.Success)
            {
                parsedCommand = null;
                return false;
            }

            var wordFindingRegex = new Regex(RegexStrings.IdentifierRegex);
            var componentMatch = wordFindingRegex.Matches(command);
            var tableName = componentMatch[TableNamePosition].Value;

            var equalsValueSubstringStartPosition = command.IndexOf(RegexStrings.EqualOperator, StringComparison.Ordinal);
            var equalsValueString = command.Substring(equalsValueSubstringStartPosition);
            var keyMatch = Regex.Match(equalsValueString, RegexStrings.RowEntryValue);
            var keyString = keyMatch.Value;

            parsedCommand = new DeleteFromCommand(tableName, keyString);
            return true;
        }
    }
}