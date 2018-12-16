using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class UseDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public UseDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.UseDatabase(DatabaseName);
            return "SUCCESS";
        }
    }
}