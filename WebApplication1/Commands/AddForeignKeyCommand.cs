using KeyValueDatabaseApi.Context;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public class AddForeignKeyCommand : ICommand
    {
        public AddForeignKeyCommand(string tableName, List<string> tableColumns, string referencedTableName, List<string> referencedTableColumns)
        {
            TableName = tableName;
            TableColumns = tableColumns;
            ReferencedTableName = referencedTableName;
            ReferencedTableColumns = referencedTableColumns;
        }

        public string TableName { get; set; }
        public List<string> TableColumns { get; set; }
        public string ReferencedTableName { get; set; }
        public List<string> ReferencedTableColumns { get; set; }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.AddForeignKey(TableName, TableColumns, ReferencedTableName, ReferencedTableColumns);
        }
    }
}