using KeyValueDatabaseApi.Context;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public CreateDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.CreateDatabase(DatabaseName);
            return "SUCCESS";
        }
    }
}