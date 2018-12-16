using System.Collections.Generic;
using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateIndexCommand : ICommand
    {
        public CreateIndexCommand(string indexName, string tableName, List<string> columnNames)
        {
            IndexName = indexName;
            TableName = tableName;
            ColumnNames = columnNames;
        }

        public string IndexName { get; }

        public string TableName { get; }

        public List<string> ColumnNames { get; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            // BUG - NU SUNT PARATE TOATE COLOANELE !
            dbContext.CreateIndex(IndexName, TableName, ColumnNames);
            return "SUCCESS";
        }
    }
}