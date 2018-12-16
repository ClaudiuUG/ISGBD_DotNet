using KeyValueDatabaseApi.Context;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectCommand : ICommand
    {
        public SelectCommand(string tableName, List<string> columnList, string keyToFind)
        {
            TableName = tableName;
            ColumnList = columnList;
            KeyToFind = keyToFind;
        }

        public SelectCommand(string tableName, string keyToFind)
        {
            TableName = tableName;
            KeyToFind = keyToFind;
            ShouldSelectAll = true;
        }

        public SelectCommand(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
        public List<string> ColumnList { get; set; }
        public bool ShouldSelectAll { get; set; }
        public string KeyToFind { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            return dbContext.SelectFromTable(TableName, ColumnList, KeyToFind);
        }
    }
}