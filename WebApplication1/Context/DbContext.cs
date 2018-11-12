using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataTanker;
using DataTanker.Settings;
using KeyValueDatabaseApi.Exceptions;
using Newtonsoft.Json;

namespace KeyValueDatabaseApi.Context
{
    public class DbContext
    {
        private static readonly string DatabasesPath = $@"C:\Users\Cristiana\Source\Repos\ISGBD_DotNet\WebApplication1\DatabaseStorage";

        private readonly string _metadataFilePath = $@"{DatabasesPath}\Metadata.json";

        public Metadata DatabaseMetadata { get; set; }

        private static readonly DbContext _dbContext = new DbContext();

        public static DbContext GetDbContext()
        {
            return _dbContext;
        }

        private DbContext()
        {
            LoadMetadataFile();
        }

        public IEnumerable<string> Databases { get; }

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

        public TableMetadataEntry GetTableFromCurrentDatabase(string tableName)
        {
            var foundTable = CurrentDatabase.Tables.SingleOrDefault(table => table.TableName.Equals(tableName));
            return foundTable ?? throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, tableName);
        }

        public void InsertRowIntoTable(string tableName, List<string> columnNames, List<string> values)
        {
            // check that the table has the specified columns - putem sa sarim peste asta momentan
            // get the type of the columns and check that the given value is of that type - putem sa sarim peste asta
            // create the object row (dynamic object)
            // cum nu abvem clase pentru fiecare row, pentru ca o tabela poate sa arate cu vrea userul
            // trebuie sa putem sa facem obiecte dimanice dupa forma tabelului pentru a le seriaza si stoca

            // Am avut probleme cu path-ul relativ, asa ca am folosit absolut, modifica DatabsePath sa functioneze si la tine
            var databaseDirectory = DatabasesPath + @"\" + CurrentDatabase.DatabaseName + @"\" + tableName;
            if (!Directory.Exists(databaseDirectory))
            {
                // se face un director cu numele bazei de date - in director o sa avem un fisier pentru fiecare tabel
                Directory.CreateDirectory(databaseDirectory);
            }

            var table = GetTableFromCurrentDatabase(tableName);

            if (table == null)
            {
                throw new TableDoesNotExistException(CurrentDatabase.DatabaseName, table.TableName);
            }

            string primaryKey = table.PrimaryKey.PrimaryKeyAttribute;
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
                    key = values[i];
                }
                else
                {
                    valuesString += "#" + values[i];
                }

            }

            var factory = new StorageFactory();
            var storage = factory.CreateBPlusTreeStorage<string, string>(BPlusTreeStorageSettings.Default(sizeof(int)));
            try
            {
                storage.OpenOrCreate(databaseDirectory);
                storage.Set(key, valuesString);

                var pair = storage.Get(key);
            }
            catch (Exception e) { }
            finally
            {
                storage.Dispose();
            }
        }
    }
}