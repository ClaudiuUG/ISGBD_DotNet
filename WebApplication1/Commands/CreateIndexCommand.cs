using System.Collections.Generic;
using KeyValueDatabaseApi.Context;
using KeyValueDatabaseApi.Exceptions;

namespace KeyValueDatabaseApi.Commands
{
    public class CreateIndexCommand : ICommand
    {
        public CreateIndexCommand(string indexName, string tableName, List<string> columnNames)
        {
            IndexName = indexName;
            TableName = tableName;
            ColumnNames = columnNames;
        }

        public string IndexName { get; }

        public string TableName { get; }

        public List<string> ColumnNames { get; }

        public void Execute()
        {
            var dbContext = DbContext.GetDbContext();
            if (dbContext.CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            var table = dbContext.GetTableFromCurrentDatabase(TableName);

            // TODO: validate that the index name is unique - maybe this should be done by the DbContext
            // TODO: Validate that the column exist in the table - maybe this should be done by the DbContext
            // TODO: move all this logic into the dbContext
            table.IndexFiles.Add(new IndexFileEntry(IndexName, ColumnNames));
            dbContext.SaveMetadataToFile();
        }
    }
}