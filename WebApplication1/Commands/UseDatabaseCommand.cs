using System.Collections.Generic;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class UseDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public UseDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.UseDatabase(DatabaseName);
        }
    }
}