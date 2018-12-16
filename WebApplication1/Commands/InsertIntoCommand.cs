using System.Collections.Generic;
using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class InsertIntoCommand : ICommand
    {
        public InsertIntoCommand(string tableName, List<string> columnNames, List<string> values)
        {
            TableName = tableName;
            ColumnNames = columnNames;
            Values = values;
        }

        public string TableName { get; }

        public List<string> ColumnNames { get; }

        public List<string> Values { get; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.InsertRowIntoTable(TableName, ColumnNames, Values);
            return "SUCCESS";
        }
    }
}