using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Matchers;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateDatabaseCommand : ICommand
    {
        public string DatabaseName { get; }

        public CreateDatabaseCommand(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.DatabaseMetadata.Databases.Add(new DatabaseMetadataEntry(null, DatabaseName));
            dbContext.SaveMetadataToFile();
        }
    }
}