using System.Collections.Generic;
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
            if (dbContext.CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            var mapper = new AttributeModelEntryMapper();
            var attributeEntries = new List<AttributeEntry>();
            foreach (var attribute in Attributes)
            {
                attributeEntries.Add(mapper.MapToEntry(attribute));
            }

            dbContext.CurrentDatabase.Tables.Add(new TableMetadataEntry(TableName, attributeEntries, null, null, null));
            dbContext.SaveMetadataToFile();
        }
    }
}