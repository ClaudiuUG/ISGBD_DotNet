using KeyValueDatabaseApi.Exceptions;
using KeyValueDatabaseApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyValueDatabaseApi.Context
{
    public class DbContext
    {
        private static readonly DbContext _dbContext = new DbContext();

        private IDbAgent _dbAgent = new DbAgent();
        private Metadata _databaseMetadata;
        private DatabaseMetadataEntry _currentDatabase;

        private DbContext()
        {
            LoadMetadataFile();
        }

        private void LoadMetadataFile()
        {
            _databaseMetadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(PathHelper.MetadataFilePath));
            if (_databaseMetadata == null)
            {
                _databaseMetadata = new Metadata(new List<DatabaseMetadataEntry>());
            }
        }

        public static DbContext GetDbContext()
        {
            return _dbContext;
        }

        public void CreateDatabase(string databaseName)
        {
            _databaseMetadata.Databases.Add(new DatabaseMetadataEntry(null, databaseName));
            SaveMetadataToFile();
        }

        public void SaveMetadataToFile()
        {
            var serializedDatabaseMetadata = JsonConvert.SerializeObject(_databaseMetadata);
            File.WriteAllText(PathHelper.MetadataFilePath, serializedDatabaseMetadata);
        }

        public void DropDatabase(string databaseName)
        {
            var databaseToRemove = GetDatabase(databaseName);
            ThrowIfDatabaseNotFound(databaseToRemove);
            _databaseMetadata.Databases.Remove(databaseToRemove);

            SaveMetadataToFile();
        }

        public DatabaseMetadataEntry GetDatabase(string databaseName)
        {
            var matchedDatabase = _databaseMetadata.Databases.SingleOrDefault(database => database.DatabaseName.Equals(databaseName));
            ThrowIfDatabaseNotFound(matchedDatabase);
            return matchedDatabase;
        }

        private void ThrowIfDatabaseNotFound(DatabaseMetadataEntry databaseMetadata)
        {
            if (databaseMetadata == null)
            {
                throw new DataBaseDoesNotExistException();
            }
        }

        public void UseDatabase(string databaseName)
        {
            _currentDatabase = GetDatabase(databaseName);
        }

        public void CreateTable(string tableName, IList<AttributeModel> attributes)
        {
            ThrowIfNoDatabaseInUse();
            ThrowIfTableNameAlreadyInUse(tableName);

            var mapper = new AttributeModelEntryMapper();
            var attributeEntries = new List<AttributeEntry>();
            foreach (var attribute in attributes)
            {
                attributeEntries.Add(mapper.MapToEntry(attribute));
            }

            var primaryKeyEntry = new PrimaryKeyEntry(attributes.First().AttributeName);
            _currentDatabase.Tables.Add(new TableMetadataEntry(tableName, attributeEntries, primaryKeyEntry));
            CreateDirectoryForTable(tableName);
            SaveMetadataToFile();
        }

        private void ThrowIfNoDatabaseInUse()
        {
            if (_currentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }
        }

        private void ThrowIfTableNameAlreadyInUse(string tableName)
        {
            var tableAlreadyExists = _currentDatabase.Tables.Any(table => table.TableName.Equals(tableName));
            if (tableAlreadyExists)
            {
                throw new TableAlreadyExistsException(tableName);
            }
        }

        private void CreateDirectoryForTable(string tableName)
        {
            var directory = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void DropTable(string tableName)
        {
            ThrowIfNoDatabaseInUse();

            var tableToRemove = GetTableFromCurrentDatabase(tableName);
            if (tableToRemove != null)
            {
                _currentDatabase.Tables.Remove(tableToRemove);
                SaveMetadataToFile();
            }
        }

        private TableMetadataEntry GetTableFromCurrentDatabase(string tableName)
        {
            var tableMetadata = _currentDatabase.Tables.SingleOrDefault(table => table.TableName.Equals(tableName));
            ThrowIfTableMetadataIsNull(tableMetadata);

            return tableMetadata;
        }

        private void ThrowIfTableMetadataIsNull(TableMetadataEntry tableMetadata)
        {
            if (tableMetadata == null)
            {
                throw new TableDoesNotExistException(_currentDatabase.DatabaseName, tableMetadata.TableName);
            }
        }

        private void ThrowIfPrimaryKeyIsNull(string primaryKey)
        {
            if (primaryKey == null)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }
        }

        public void InsertRowIntoTable(string tableName, List<string> columnNames, List<string> values)
        {
            ThrowIfNoDatabaseInUse();

            var tableMetadata = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(tableMetadata);

            var primaryKey = tableMetadata.PrimaryKey.PrimaryKeyAttribute;
            ThrowIfPrimaryKeyIsNull(primaryKey);

            // TODO: check the eventual foreign key constraints
            // get the foreign keys
            // check that the value from the referenced table columns exist in the parent table for each foreign key
            // how ? indexes - each index should have a b+ tree kept consistent upon insertion
            // beside this, need to update index creation and search using indexes, no other way
            // if the foreign key constraints are not fulfilled, throw an error with a message

            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            var keyValueToInsert = CreateKeyValueForData(columnNames, values, primaryKey);
            ThrowIfForeignKeyConstraintsAreNotMet(tableName);

            _dbAgent.InsertIntoStorage(tablePath, keyValueToInsert.Key, keyValueToInsert.Value);
            UpdateIndices(tableMetadata, values, columnNames);
            UpdateUniqueIndices(tableMetadata, values, columnNames);
            UpdateForeignKeys(tableMetadata, values, columnNames);

            SaveMetadataToFile();
        }

        private KeyValuePair<string, string> CreateKeyValueForData(List<string> columnNames, List<string> values, string primaryKey)
        {
            var key = string.Empty;
            var valuesString = string.Empty;

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(columnNames[i], primaryKey))
                {
                    key += "#" + values[i];
                }
                valuesString += "#" + values[i];
            }

            return new KeyValuePair<string, string>(key, valuesString);
        }

        public void ThrowIfForeignKeyConstraintsAreNotMet(string tableName)
        {
            var foreignKeys = GetForeignKeysForTable(tableName);
            foreach (var foreignKey in foreignKeys)
            {
                var tableColumns = foreignKey.Columns;
                var referencedTable = foreignKey.ReferencedTableName;
                var referencedColumns = foreignKey.ReferencedTableColumns;
                // should use or create index on the referenced table to check that there are entries that meet the constratint
                // first, we should have working indexes
            }
        }

        public List<ForeignKeyEntry> GetForeignKeysForTable(string tableName)
        {
            var table = GetTableFromCurrentDatabase(tableName);
            return table.ForeignKeys;
        }

        public void UpdateIndices(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            var key = string.Empty;
            var valuesString = string.Empty;
            foreach (var indexFile in tableMetadata.IndexFiles)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (indexFile.IndexAttributes.Contains(columnNames[i]))
                    {
                        key += "#" + values[i];
                    }
                    valuesString += "#" + values[i];
                }

                var indexStoragePath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, indexFile.IndexName);
                _dbAgent.InsertIntoStorage(indexStoragePath, key, valuesString);
            }
        }

        public void UpdateUniqueIndices(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            var key = string.Empty;
            var valuesString = string.Empty;
            foreach (var entry in tableMetadata.UniqueKeyEntry)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (entry.UniqueAttribute.Equals(columnNames[i]))
                    {
                        key = values[i];
                    }
                    valuesString += "#" + values[i];
                }

                var indexStoragePath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, entry.UniqueAttribute);
                _dbAgent.InsertIntoStorage(indexStoragePath, key, valuesString);
            }
        }

        public void UpdateForeignKeys(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            // Don't quite get this one.
            var key = string.Empty;
            var valuesString = string.Empty;
            foreach (var entry in tableMetadata.ForeignKeys)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (entry.Columns.FirstOrDefault().Equals(columnNames[i]))
                    {
                        if (SelectRowFromTable(entry.ReferencedTableName, null, values[i]).Count == 0)
                        {
                            // This is called after everything else is inserted, throwing here would leave the database in a bad state
                            // Since this is part of the row add and it checks constraints, it should be called the first.
                            throw new Exception("Key does not exist");
                        }

                        key = values[i];
                    }
                    valuesString += "#" + values[i];
                }

                var referencedTableStoragePath = PathHelper.GetReferencedTablePath(_currentDatabase.DatabaseName, tableMetadata.TableName, entry.ReferencedTableName);
                _dbAgent.InsertIntoStorage(referencedTableStoragePath, key, valuesString);
            }
        }

        public void DeleteRowFromTable(string tableName, string key)
        {
            ThrowIfNoDatabaseInUse();

            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            var tableMetadata = GetTableFromCurrentDatabase(tableName);

            key = "#" + key;
            ThrowIfDatabaseDirectoryNotFound(tablePath, tableMetadata);
            ThrowIfTableMetadataIsNull(tableMetadata);

            var primaryKey = tableMetadata.PrimaryKey.PrimaryKeyAttribute;
            ThrowIfPrimaryKeyIsNull(primaryKey);

            _dbAgent.DeleteFromStorage(tablePath, key);
            SaveMetadataToFile();
        }

        private void ThrowIfDatabaseDirectoryNotFound(string databaseDirectory, TableMetadataEntry tableMetadata)
        {
            if (!Directory.Exists(databaseDirectory))
            {
                throw new TableDoesNotExistException(_currentDatabase.DatabaseName, tableMetadata.TableName);
            }
        }

        public string SelectFromTable(string tableName, List<string> columnList, string keyToFind)
        {
            ThrowIfNoDatabaseInUse();
            var resultTableRows = SelectRowFromTable(tableName, columnList, keyToFind);
            return string.Join(" ", resultTableRows);
        }

        private List<string> SelectRowFromTable(string tableName, List<string> columnNames, string searchedKeyValue)
        {
            var tableMetadata = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(tableMetadata);

            var key = "#" + searchedKeyValue;
            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            var resultRow = _dbAgent.GetFromStorage(tablePath, key);
            return FormatResultRow(resultRow, columnNames, tableMetadata);
        }

        private List<string> FormatResultRow(string row, List<string> columnNames, TableMetadataEntry tableMetadata)
        {
            var result = new List<string>();
            var values = row.Split('#');
            for (var i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]))
                {
                    continue;
                }
                if (columnNames == null || columnNames.Contains(tableMetadata.Structure.ElementAt(i).AttributeName) || columnNames.Count == 0)
                {
                    result.Add(values[i]);
                }
            }

            return result;
        }

        public void CreateIndex(string indexName, string tableName, List<string> columnNames)
        {
            ThrowIfNoDatabaseInUse();

            var table = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableDoesNotExist(tableName);

            var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableName, indexName);
            ThrowIfIndexAlreadyExists(indexPath, tableName);

            ThrowIfIndexingColumnsDoNotExist(table, columnNames);

            // BUILD INDEX
            // multiple inserts into storage for index path
            // key = valoarea coloanelor
            // valoare = restul campurilor concatenate

            table.IndexFiles.Add(new IndexFileEntry(indexName, columnNames));
            SaveMetadataToFile();
        }

        private void ThrowIfTableDoesNotExist(string tableName)
        {
            if (!FileHelper.CheckFileExists(tableName))
            {
                throw new TableDoesNotExistException(_currentDatabase.DatabaseName, tableName);
            }
        }

        private void ThrowIfIndexAlreadyExists(string indexPath, string tableName)
        {
            if (FileHelper.CheckFileExists(indexPath))
            {
                throw new IndexAlreadyExistsException(_currentDatabase.DatabaseName, tableName, indexPath);
            }
        }

        private void ThrowIfIndexingColumnsDoNotExist(TableMetadataEntry tableMetadata, List<string> indexColumnNames)
        {
            foreach (var indexColumnName in indexColumnNames)
            {
                if (!tableMetadata.Structure.Any(attributeEntry => attributeEntry.AttributeName.Equals(indexColumnName)))
                {
                    throw new IndexingColumnDoesNotExist(_currentDatabase.DatabaseName, tableMetadata.TableName, indexColumnName);
                }
            }
        }

        public void AddForeignKey(string tableName, List<string> tableColumns, string referencedTableName, List<string> referencedTableColumns)
        {
            ThrowIfNoDatabaseInUse();

            var table = GetTableFromCurrentDatabase(tableName);
            ForeignKeyEntry foreignKeyEntry = new ForeignKeyEntry(tableColumns, referencedTableName, referencedTableColumns);
            table.ForeignKeys.Add(foreignKeyEntry);
            SaveMetadataToFile();
        }
    }
}
