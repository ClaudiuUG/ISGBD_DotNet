using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;

namespace KeyValueDatabaseApi.Commands
{
    public class DeleteFromCommand : ICommand
    {
        public DeleteFromCommand(string tableName, string key)
        {
            TableName = tableName;
            Key = key;
        }

        public string TableName { get; }
           
        public string Key { get; }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (dbContext.CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            dbContext.DeleteRowFromTable(TableName, Key);
            dbContext.SaveMetadataToFile();
        }
    }
}