using System.Collections.Generic;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Models;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateTableCommand : ICommand
    {
        public CreateTableCommand(string tableName, List<AttributeModel> attributes)
        {
            TableName = tableName;
            Attributes = attributes;
        }

        public string TableName { get; }

        public List<AttributeModel> Attributes { get; }

        public string Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.CreateTable(TableName, Attributes);
            return "SUCCESS";
        }
    }
}