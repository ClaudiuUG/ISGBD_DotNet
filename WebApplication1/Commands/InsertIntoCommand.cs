using System.Collections.Generic;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;

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

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (dbContext.CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            dbContext.InsertRowIntoTable(TableName, ColumnNames, Values);
            dbContext.SaveMetadataToFile();
        }
    }
}