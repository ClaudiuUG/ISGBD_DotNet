using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public enum AggregatorFunction
    {
        Count,
        Sum
    }

    public class SelectCommand : ICommand
    {
        public SelectCommand(string tableName, List<string> columnList, string keyColumn, string keyValue) : this(
            tableName, keyColumn, keyValue)
        {
            ColumnList = columnList;
        }

        public SelectCommand(string tableName, string keyColumn, string keyValue) : this(tableName)
        {
            KeyColumn = keyColumn;
            KeyValue = keyValue;
        }

        public SelectCommand(string tableName, string groupByColumn, int? groupByColumnValue,
            string groupByHavingComparer, AggregatorFunction aggregatorFunction) :
            this(tableName, groupByColumn, aggregatorFunction)
        {
            GroupByColumnValue = groupByColumnValue;
            GroupByHavingComparer = groupByHavingComparer;
        }

        public SelectCommand(string tableName, string groupByColumn, AggregatorFunction aggregatorFunction) : this(tableName)
        {
            GroupByColumn = groupByColumn;
            AggregatorFunction = aggregatorFunction;
        }

        public SelectCommand(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
        public List<string> ColumnList { get; set; }
        public string KeyColumn { get; set; }
        public string KeyValue { get; set; }
        public string GroupByColumn { get; set; }
        public int? GroupByColumnValue { get; set; } = null;
        public string GroupByHavingComparer { get; set; }
        public AggregatorFunction AggregatorFunction { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (string.IsNullOrEmpty(GroupByColumn))
            {
                return dbContext.SelectFromTable(TableName, ColumnList, KeyColumn, KeyValue);
            }

            if (!string.IsNullOrEmpty(GroupByColumn))
            {
                if (AggregatorFunction == AggregatorFunction.Count)
                {
                    if (GroupByColumnValue == null)
                    {
                        return dbContext.GroupByHavingCount(TableName, GroupByColumn, GroupByColumnValue.Value, GroupByHavingComparer);
                    }
                    else
                    {
                        return dbContext.GroupByCount(TableName, GroupByColumn);
                    }
                }
                if (AggregatorFunction == AggregatorFunction.Sum)
                {
                    if (GroupByColumnValue == null)
                    {
                        return dbContext.GroupByHavingSum(TableName, GroupByColumn, GroupByColumnValue.Value, GroupByHavingComparer);
                    }
                    else
                    {
                        return dbContext.GroupBySum(TableName, GroupByColumn);
                    }
                }
            }

            throw new SelectCommandExecutionException();
        }
    }
}