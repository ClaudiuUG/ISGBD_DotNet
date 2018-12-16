using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class DropDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public DropDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.DropDatabase(DatabaseName);
            return "SUCCESS";
        }
    }
}