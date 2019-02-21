using System.Collections.Generic;
using System.Linq;
using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public enum JoinDirection
    {
        Left,
        Right
    }

    public class SelectOuterJoinCommand : ICommand
    {
        public SelectOuterJoinCommand(string firstTable, string secondTable, string firstJoinColumn,
            string secondJoinColumn, IEnumerable<string> columnNames, JoinDirection joinDirection)
        {
            FirstTable = firstTable;
            SecondJoinColumn = secondTable;
            FirstJoinColumn = firstJoinColumn;
            SecondJoinColumn = secondJoinColumn;
            ColumnNames = columnNames;
            JoinDirection = joinDirection;
        }

        public string FirstTable { get; set; }
        public string SecondTable { get; set; }
        public string FirstJoinColumn { get; set; }
        public string SecondJoinColumn { get; set; }
        public IEnumerable<string> ColumnNames { get; set; }
        public JoinDirection JoinDirection { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (JoinDirection == JoinDirection.Left)
            {
                return dbContext.SelectFromLeftOuterJoinedTables(FirstTable, SecondTable, FirstJoinColumn, SecondJoinColumn,
                    ColumnNames.ToList()).Aggregate((l, r) => l + "\n");
            }
            return dbContext.SelectFromRightOuterJoinedTables(FirstTable, SecondTable, FirstJoinColumn, SecondJoinColumn, 
                ColumnNames.ToList()).Aggregate((l, r) => l + "\n");
        }
    }
}