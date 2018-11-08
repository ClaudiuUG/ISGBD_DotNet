using System;
using KeyValueDatabaseApi.Exceptions;

namespace KeyValueDatabaseApi.Validators
{
    public class CommandValidator : ICommandValidator
    {
        private char[] _illegalCharacters = { '/', '\\' };

        public void ValidateCommand (string command) {
            var invalidCharacterPosition = command.IndexOfAny(_illegalCharacters);
            if (invalidCharacterPosition != -1) {
                var errorMessage = "Syntax failure: Illegal character found at zero-based position " + invalidCharacterPosition;
                Console.WriteLine(errorMessage);
                throw new InvalidCommandException(errorMessage);
            }
        }
    }
}