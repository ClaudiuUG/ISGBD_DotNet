using KeyValueDatabaseApi.Context;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectCommand : ICommand
    {
        public SelectCommand(string tableName, List<string> columnList, string keyColumn, string keyValue)
        {
            TableName = tableName;
            ColumnList = columnList;
            KeyColumn = keyColumn;
            KeyValue = keyValue;
        }

        public SelectCommand(string tableName, string keyColumn, string keyValue)
        {
            TableName = tableName;
            KeyColumn = keyColumn;
            KeyValue = keyValue;
            ShouldSelectAll = true;
        }

        public SelectCommand(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
        public List<string> ColumnList { get; set; }
        public bool ShouldSelectAll { get; set; }
        public string KeyColumn { get; set; }
        public string KeyValue { get; set; }
        public string GroupByColumn { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (string.IsNullOrEmpty(GroupByColumn))
            {
                return dbContext.SelectFromTable(TableName, ColumnList, KeyColumn, KeyValue);
            }
            return dbContext.GroupByCount(TableName, GroupByColumn);
        }
    }
}