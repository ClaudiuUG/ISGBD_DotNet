using System.Linq;
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
            var databaseToRemove = dbContext.GetDatabase(DatabaseName);
            if (databaseToRemove != null)
            {
                dbContext.DatabaseMetadata.Databases.Remove(databaseToRemove);
            }
            dbContext.SaveMetadataToFile();
        }
    }
}