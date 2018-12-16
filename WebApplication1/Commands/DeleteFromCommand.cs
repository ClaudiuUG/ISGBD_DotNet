using KeyValueDatabaseApi.Context;

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

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.DeleteRowFromTable(TableName, Key);
            return "SUCCESS";
        }
    }
}