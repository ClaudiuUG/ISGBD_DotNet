using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class DropTableCommand : ICommand
    {
        public DropTableCommand(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.DropTable(TableName);
            return "SUCCESS";
        }
    }
}