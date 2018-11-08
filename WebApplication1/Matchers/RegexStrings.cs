namespace KeyValueDatabaseApi.Matchers
{
    public static class RegexStrings
    {
        public static string CreateCommandRegex = @"\s*create\s*";
        public static string DropCommandRegex = @"\s*drop\s*";
        public static string InsertIntoCommandRegex = @"\s*insert\s*into\s*";
        public static string UseCommandRegex = @"\s*use\s*";
        public static string DatabaseReservedWordRegex = @"\s*database\s*";
        public static string TableReservedWordRegex = @"\s*table\s*";
        public static string IndexReservedWordRegex = @"\s*index\s*";
        public static string OnReservedWordRegex = @"\s*on\s*";
        public static string ValuesReservedKeyword = @"\s*values\s*";
        public static string IdentifierRegex = "[A-Za-z0-9][A-Za-z0-9]*";
        public static string IdentifierRegexWithSpaces = @"\s*[A-Za-z][A-Za-z0-9]*\s*";
        public static string ParameterListWithoutParanthesisRegex = $@"{IdentifierRegexWithSpaces}(,{IdentifierRegexWithSpaces})*";
        public static string ParameterListRegex = $@"\s*\({ParameterListWithoutParanthesisRegex}\)\s*";
        public static string ColumnTypeRegex = @"\s*((string)|(int)|(double)|(float))\s*";
        public static string ColumnNameAndTypeRegex = $@"\s*{IdentifierRegex}\s*{ColumnTypeRegex}\s*";
        public static string TableColumnsRegex = $@"\s*\({ColumnNameAndTypeRegex}(,{ColumnNameAndTypeRegex})*\)";
        public static string TableColumnsWithoutParanthesisRegex = $@"{ColumnNameAndTypeRegex}(,{ColumnNameAndTypeRegex})+?";
        public static string RowEntryValue = @"[A-Za-z0-9]+";
        public static string RowEntryValueWithSpaces = $@"\s*{RowEntryValue}\s*";
        public static string RowEntryValueListWithoutParanthesisRegex = $@"{RowEntryValueWithSpaces}(,{RowEntryValueWithSpaces})*";
        public static string RowEntryValueListRegex = $@"\s*\({RowEntryValueListWithoutParanthesisRegex}\)\s*";
    }
}