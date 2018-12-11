using System.Linq;
using DataTanker;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;

namespace KeyValueDatabaseApi.Commands
{
    public class DropDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public DropDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.DropDatabase(DatabaseName);
        }
    }
}