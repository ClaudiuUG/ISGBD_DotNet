using DataTanker;
using DataTanker.Settings;
using KeyValueDatabaseApi.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyValueDatabaseApi.Context
{
    public class DbContext : IDisposable
    {
        private static readonly string DatabasesPath = $@"C:\Users\Cristiana\Source\Repos\ISGBD_DotNet\WebApplication1\DatabaseStorage";
        //private static readonly string DatabasesPath = $@"C:\Users\Claudiu\source\repos\WebApplication1\WebApplication1\DatabaseStorage";

        private readonly string _metadataFilePath = $@"{DatabasesPath}\Metadata.json";
        private static StorageFactory _storageFactory;

        private IKeyValueStorage<ComparableKeyOf<string>, ValueOf<string>> _storage;
        private static readonly DbContext _dbContext = new DbContext();

        private DbContext()
        {
            LoadMetadataFile();
            _storageFactory = new StorageFactory();
            _storage = _storageFactory.CreateBPlusTreeStorage<string, string>(BPlusTreeStorageSettings.Default(sizeof(int)));
        }

        public Metadata DatabaseMetadata { get; set; }

        public static DbContext GetDbContext()
        {
            return _dbContext;
        }

        public DatabaseMetadataEntry CurrentDatabase { get; set; }

        private void LoadMetadataFile()
        {
            DatabaseMetadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(_metadataFilePath));
            if (DatabaseMetadata == null)
            {
                DatabaseMetadata = new Metadata(new List<DatabaseMetadataEntry>());
            }
        }

        public void SaveMetadataToFile()
        {
            var serializedDatabaseMetadata = JsonConvert.SerializeObject(DatabaseMetadata);
            File.WriteAllText(_metadataFilePath, serializedDatabaseMetadata);
        }

        public void UseDatabase(string databaseName)
        {
            _dbContext.CurrentDatabase = GetDatabase(databaseName);
        }

        public DatabaseMetadataEntry GetDatabase(string databaseName)
        {
            var matchedDatabase = DatabaseMetadata.Databases.SingleOrDefault(database => database.DatabaseName.Equals(databaseName));
            return matchedDatabase ?? throw new DataBaseDoesNotExistException();
        }

        public void InsertRowIntoTable(string tableName, List<string> columnNames, List<string> values)
        {
            var tablePath = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            if (!Directory.Exists(tablePath))
            {
                Directory.CreateDirectory(tablePath);
            }
            var tableMetadata = GetTableFromCurrentDatabase(tableName);
            if (!_storage.IsOpen)
            {
                _storage.OpenOrCreate(tablePath);
            }

            if (tableMetadata == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, tableMetadata.TableName);
            }

            var primaryKey = tableMetadata.PrimaryKey.PrimaryKeyAttribute;
            if (primaryKey == null)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }

            // TODO: check the eventual foreign key constraints
            // get the foreign keys
            // check that the value from the referenced table columns exist in the parent table for each foreign key
            // how ? indexes - each index should have a b+ tree kept consistent upon insertion
            // beside this, need to update index creation and search using indexes, no other way
            // if the foreign key constraints are not fulfilled, throw an error with a message

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

            InsertIntoIndexFile(tablePath, key, valuesString);

            key = string.Empty;
            valuesString = string.Empty;
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

                InsertIntoIndexFile(DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\index\" + tableName + @"\" + indexFile.IndexName, key, valuesString);
            }

            key = string.Empty;
            valuesString = string.Empty;
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

                InsertIntoIndexFile(DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\index\" + tableName + @"\" + entry.UniqueAttribute, key, valuesString);
            }

            key = string.Empty;
            valuesString = string.Empty;
            foreach (var entry in tableMetadata.ForeignKeys)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (entry.Columns.FirstOrDefault().Equals(columnNames[i]))
                    {
                        if(SelectRowFromTable(entry.ReferencedTableName, null, values[i]).Count == 0)
                        {
                            throw new Exception("Key does not exist");
                        }

                        key = values[i];
                    }
                    valuesString += "#" + values[i];
                }

                InsertIntoIndexFile(DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\index\" + tableName + @"\" + entry.ReferencedTableName, key, valuesString);
            }

        }

        private TableMetadataEntry GetTableFromCurrentDatabase(string tableName)
        {
            var foundTable = CurrentDatabase.Tables.SingleOrDefault(table => table.TableName.Equals(tableName));
            return foundTable ?? throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, tableName);
        }

        public void DeleteRowFromTable(string tableName, string key)
        {
            var databaseDirectory = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            var table = GetTableFromCurrentDatabase(tableName);

            key = "#" + key;
            if (!Directory.Exists(databaseDirectory))
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            if (table == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            var primaryKey = table.PrimaryKey.PrimaryKeyAttribute;
            if (primaryKey == null)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }

            DeleteFromIndexFile(databaseDirectory, key);
        }

        public List<string> SelectRowFromTable(string tableName, List<string> columnNames, string searchedKeyValue)
        {
            var databaseDirectory = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            var table = GetTableFromCurrentDatabase(tableName);
            var key = string.Empty;

            if (table == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            //if (!_storage.IsOpen)
            //{
            _storage.Close();
            _storage.OpenOrCreate(databaseDirectory);
            //}

            key = "#" + searchedKeyValue;

            var result = new List<string>();
            if (string.IsNullOrEmpty(key))
            {
                return result;
            }
            if (!_storage.Exists(key))
            {
                return result;
            }

            var row = _storage.Get(key).Value;
            var values = row.Split('#');
            for (var i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrEmpty(values[i]))
                {
                    continue;
                }
                if (columnNames == null || columnNames.Contains(table.Structure.ElementAt(i).AttributeName) || columnNames.Count == 0)
                {
                    result.Add(values[i]);
                }
            }

            return result;
        }

        private void InsertIntoIndexFile(string directory, string key, string value)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!_storage.IsOpen)
                _storage.OpenOrCreate(directory);

            _storage.Set(key, value);
        }

        private void DeleteFromIndexFile(string directory, string key)
        {
            if (!_storage.IsOpen)
                _storage.OpenOrCreate(directory);

            _storage.Remove(key);
        }

        public void DropTable(string tableName)
        {
            if (CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            var tableToRemove = GetTableFromCurrentDatabase(tableName);
            if (tableToRemove != null)
            {
                CurrentDatabase.Tables.Remove(tableToRemove);
                SaveMetadataToFile();
            }
        }

        internal void CreateIndex(string indexName, string tableName, List<string> columnNames)
        {
            if (CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            // TODO: validate that the index name is unique - maybe this should be done by the DbContext
            // TODO: Validate that the column exist in the table - maybe this should be done by the DbContext
            // TODO: move all this logic into the dbContext
            // the index should have a b+ tree asociated
            var table = GetTableFromCurrentDatabase(tableName);
            table.IndexFiles.Add(new IndexFileEntry(indexName, columnNames));
            SaveMetadataToFile();
        }

        public void AddForeignKey(string tableName, List<string> tableColumns, string referencedTableName, List<string> referencedTableColumns)
        {
            if (CurrentDatabase == null)
            {
                throw new NoDatabaseInUseException();
            }

            var table = GetTableFromCurrentDatabase(tableName);
            ForeignKeyEntry foreignKeyEntry = new ForeignKeyEntry(tableColumns, referencedTableName, referencedTableColumns);
            table.ForeignKeys.Add(foreignKeyEntry);
            SaveMetadataToFile();
        }

        public void Dispose()
        {
            _storage.Dispose();
        }
    }
}
