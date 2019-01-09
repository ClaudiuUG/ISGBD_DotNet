using RaptorDB;

namespace KeyValueDatabaseApi.Matchers
{
    public static class RegexStrings
    {
        public static string CreateCommandRegex = @"\s*create\s*";
        public static string DropCommandRegex = @"\s*drop\s*";
        public static string InsertIntoReservedWordsRegex = @"\s*insert\s*into\s*";
        public static string UseCommandRegex = @"\s*use\s*";
        public static string DatabaseReservedWordRegex = @"\s*database\s*";
        public static string TableReservedWordRegex = @"\s*table\s*";
        public static string IndexReservedWordRegex = @"\s*index\s*";
        public static string OnReservedWordWithoutSpacesRegex = "on";
        public static string OnReservedWordRegex = $@"\s*{OnReservedWordWithoutSpacesRegex}\s*";
        public static string ValuesReservedKeyword = @"\s*values\s*";
        public static string IdentifierRegex = "[A-Za-z0-9][A-Za-z0-9]*";
        public static string IdentifierRegexWithSpaces = @"\s*[A-Za-z][A-Za-z0-9]*\s*";
        public static string ParameterListWithoutParenthesesRegex = $@"{IdentifierRegexWithSpaces}(,{IdentifierRegexWithSpaces})*";
        public static string ParameterListRegex = $@"\s*\({ParameterListWithoutParenthesesRegex}\)\s*";
        public static string ColumnTypeRegex = @"\s*((string)|(int)|(double)|(float))\s*";
        public static string ColumnNameAndTypeRegex = $@"\s*{IdentifierRegex}\s*{ColumnTypeRegex}\s*";
        public static string TableColumnsRegex = $@"\s*\({ColumnNameAndTypeRegex}(,{ColumnNameAndTypeRegex})*\)";
        public static string TableColumnsWithoutParenthesesRegex = $@"{ColumnNameAndTypeRegex}(,{ColumnNameAndTypeRegex})+?";
        public static string RowEntryValue = @"[A-Za-z0-9]+";
        public static string RowEntryValueWithSpaces = $@"\s*{RowEntryValue}\s*";
        public static string RowEntryValueListWithoutParenthesisRegex = $@"{RowEntryValueWithSpaces}(,{RowEntryValueWithSpaces})*";
        public static string RowEntryValueListRegex = $@"\s*\({RowEntryValueListWithoutParenthesisRegex}\)\s*";
        public static string FromReservedWordWithoutSpacesRegex = "from";
        public static string FromReservedWordRegex = $@"\s*{FromReservedWordWithoutSpacesRegex}\s*";
        public static string DeleteFromReservedWordsRegex = $@"\s*delete{FromReservedWordRegex}\";
        public static string WhereReservedWordWithoutSpacesRegex = "where";
        public static string WhereReservedWordRegex = @"\s*where\s*";
        public static string KeyReservedWord = "key";
        public static string KeyReservedWordRegex = $@"\s*{KeyReservedWord}\s*";
        public static string EqualOperator = "=";
        public static string EqualOperatorRegex = $@"\s*{EqualOperator}\s*";
        public static string KeyEqualsValueRegex = $@"{KeyReservedWordRegex}=\s*{RowEntryValue}";
        public static string AlterReservedWordRegex = @"\s*alter\s*";
        public static string AlterTableReservedWordsRegex = $"{AlterReservedWordRegex}{TableReservedWordRegex}";
        public static string AddReservedWordRegex = @"\s*add\s*";
        public static string ForeignReservedWordRegex = @"\s*foreign\s*";
        public static string AddForeignKeyReservedWordsRegex = $"{AddReservedWordRegex}{ForeignReservedWordRegex}{KeyReservedWordRegex}";
        public static string ReferencesWithoutSpacesRegex = "references";
        public static string ReferencesReservedWordRegex = $@"\s*{ReferencesWithoutSpacesRegex}\s*";
        public static string SelectReservedWordWithoutSpacesRegex = "select";
        public static string SelectReservedWordRegex = $@"\s*{SelectReservedWordWithoutSpacesRegex}\s*";
        public static string SelectSubject = @"\*";
        public static string JoinTypeWithoutSpacesRegex = @"loop|hash";
        public static string JoinTypeRegex = $@"\s*{JoinTypeWithoutSpacesRegex}\s*";
        public static string JoinWithoutSpacesRegex = "join";
        public static string JoinReservedWordRegex = $@"\s*{JoinWithoutSpacesRegex}\s*";
        public static string PrefixedColumnWithoutSpacesRegex = IdentifierRegex + "." + IdentifierRegex;
        public static string PrefixedColumnRegex = $@"\s*{PrefixedColumnWithoutSpacesRegex}\s*";
    }
}