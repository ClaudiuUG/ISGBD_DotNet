using System.Web.UI.WebControls;
using DataTanker;
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
            dbContext.DropTable(TableName);
        }
    }
}