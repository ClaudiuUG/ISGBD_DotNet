using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;

namespace KeyValueDatabaseApi.Commands
{
    public class DropTableCommand : ICommand
    {
        public DropTableCommand(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
             if (dbContext.CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            var tableToRemove = dbContext.GetTableFromCurrentDatabase(TableName);
            if (tableToRemove != null)
            {
                dbContext.CurrentDatabase.Tables.Remove(tableToRemove);
                dbContext.SaveMetadataToFile();
            }
        }
    }
}