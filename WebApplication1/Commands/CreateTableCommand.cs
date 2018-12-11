using System.Collections.Generic;
using DataTanker;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;
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

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            dbContext.CreateTable(TableName, Attributes);
        }
    }
}