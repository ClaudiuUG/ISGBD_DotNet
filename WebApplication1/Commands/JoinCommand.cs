using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public enum JoinType
    {
        IndexNestedLoopJoin,
        HashJoin
    }

    public class JoinCommand : ICommand
    {
        public JoinCommand(string firstTableName, string secondTableName, string firstColumnName,
            string secondColumnName, List<string> columnNames, JoinType joinType)
        {
            FirstTableName = firstTableName;
            SecondTableName = secondTableName;
            FirstColumnName = firstColumnName;
            SecondColumnName = secondColumnName;
            ColumnNames = columnNames;
            JoinType = joinType;
        }

        public string FirstTableName { get; }
        public string SecondTableName { get; }
        public string FirstColumnName { get; }
        public string SecondColumnName { get; }
        public List<string> ColumnNames { get; }
        public JoinType JoinType { get; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (JoinType == JoinType.IndexNestedLoopJoin)
            {
                return dbContext.IndexedNestedLoopJoin(FirstTableName, SecondTableName, FirstColumnName, SecondColumnName, ColumnNames);
            }

            return dbContext.HashJoin(FirstTableName, SecondTableName, FirstColumnName, SecondColumnName, ColumnNames);
        }
    }
}