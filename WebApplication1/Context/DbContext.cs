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
        // private static readonly string DatabasesPath = $@"C:\Users\Cristiana\Source\Repos\ISGBD_DotNet\WebApplication1\DatabaseStorage";
        private static readonly string DatabasesPath =
            $@"C:\Users\Claudiu\source\repos\WebApplication1\WebApplication1\DatabaseStorage";

        private readonly string _metadataFilePath = $@"{DatabasesPath}\Metadata.json";

        private static StorageFactory _factory;

        private IKeyValueStorage<ComparableKeyOf<string>, ValueOf<string>> storage;

        public Metadata DatabaseMetadata { get; set; }

        private static readonly DbContext _dbContext = new DbContext();

        public static DbContext GetDbContext()
        {
            return _dbContext;
        }

        private DbContext()
        {
            LoadMetadataFile();
            _factory = new StorageFactory();
            storage = _factory.CreateBPlusTreeStorage<string, string>(BPlusTreeStorageSettings.Default(sizeof(int)));
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
            // SelectRowFromTable("demotable", new List<string> { "columnone, columntwo" }, "column");
        }

        public DatabaseMetadataEntry GetDatabase(string databaseName)
        {
            var matchedDatabase = DatabaseMetadata.Databases.SingleOrDefault(database => database.DatabaseName.Equals(databaseName));
            return matchedDatabase ?? throw new DataBaseDoesNotExistException();
        }

        public TableMetadataEntry GetTableFromCurrentDatabase(string tableName)
        {
            var foundTable = CurrentDatabase.Tables.SingleOrDefault(table => table.TableName.Equals(tableName));
            return foundTable ?? throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, tableName);
        }

        public void InsertRowIntoTable(string tableName, List<string> columnNames, List<string> values)
        {
            var databaseDirectory = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            var table = GetTableFromCurrentDatabase(tableName);
            if(!storage.IsOpen)
                storage.OpenOrCreate(databaseDirectory);

            if (table == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            var primaryKey = table.PrimaryKey.PrimaryKeyAttribute;
            if (primaryKey == null)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }

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

            InsertIntoIndexFile(databaseDirectory, key, valuesString);

            key = string.Empty;
            valuesString = string.Empty;
            foreach (var indexFile in table.IndexFiles)
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
            foreach (var entry in table.UniqueKeyEntry)
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

        public List<string> SelectRowFromTable(string tableName, List<string> columnNames, string conditionValue)
        {
            var databaseDirectory = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            var table = GetTableFromCurrentDatabase(tableName);
            var key = string.Empty;

            if (table == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            if (!storage.IsOpen)
                storage.OpenOrCreate(databaseDirectory);

            key = "#" + conditionValue;

            var result = new List<string>();
            if (string.IsNullOrEmpty(key))
            {
                return result;
            }
            if (!storage.Exists(key))
            {
                return result;
            }

            string row = storage.Get(key);
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

            if (!storage.IsOpen)
                storage.OpenOrCreate(directory);

            storage.Set(key, value);
        }

        private void DeleteFromIndexFile(string directory, string key)
        {
            if (!storage.IsOpen)
                storage.OpenOrCreate(directory);

            storage.Remove(key);
        }

        public void Dispose()
        {
            storage.Dispose();
        }
    }
}
