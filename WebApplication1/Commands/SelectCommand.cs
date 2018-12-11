using DataTanker;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;
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
            throw new NotImplementedException();
        }

        public string ExecuteSelect()
        {
            var dbContext = DbContext.GetDbContext();
            return dbContext.SelectFromTable(TableName, ColumnList, KeyToFind);
        }

    }
}