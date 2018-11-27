using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public void Execute()
        {
            // update this method if we ever want to support select with projection
            // throw new NotImplementedException();
        }
    }
}