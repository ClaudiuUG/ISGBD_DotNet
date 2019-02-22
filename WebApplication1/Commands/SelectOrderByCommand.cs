using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class SelectOrderByCommand : ICommand
    {
        public SelectOrderByCommand(string tableName, string columnBy)
        {
            TableName = tableName;
            ColumnBy = columnBy;
        }

        public string TableName { get; set; }
        public string ColumnBy { get; set; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            return dbContext.SelectOrderBy(TableName, ColumnBy);
        }
    }
}