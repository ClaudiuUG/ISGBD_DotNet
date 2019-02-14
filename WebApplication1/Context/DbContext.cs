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

        private IDbAgent _dbAgent = new DbAgentBPlus();
        private Metadata _databaseMetadata;
        private DatabaseMetadataEntry _currentDatabase;

        private DbContext()
        {
            LoadMetadataFile();
        }

        #region Commands
        public void CreateDatabase(string databaseName)
        {
            _databaseMetadata.Databases.Add(new DatabaseMetadataEntry(null, databaseName));
            SaveMetadataToFile();
        }

        public void UseDatabase(string databaseName)
        {
            _currentDatabase = GetDatabase(databaseName);
        }

        public void DropDatabase(string databaseName)
        {
            var databaseToRemove = GetDatabase(databaseName);
            ThrowIfDatabaseNotFound(databaseToRemove);
            _databaseMetadata.Databases.Remove(databaseToRemove);

            SaveMetadataToFile();
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
            SaveMetadataToFile();
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

        public void InsertRowIntoTable(string tableName, List<string> columnNames, List<string> values)
        {
            ThrowIfNoDatabaseInUse();

            var tableMetadata = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(tableMetadata, tableName);

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

            ThrowIfForeignKeyConstraintsAreNotMet(tableMetadata, columnNames, values);
            ThrowIfUniqueConstraintsAreNotMet(tableMetadata, values, columnNames);
            ThrowIfKeyAlreadyStored(tablePath, keyValueToInsert.Key);

            _dbAgent.InsertIntoStorage(tablePath, keyValueToInsert.Key, keyValueToInsert.Value);
            UpdateIndices(tableMetadata, values, columnNames);
            UpdateUniqueIndices(tableMetadata, values, columnNames);
            UpdateForeignKeys(tableMetadata, values, columnNames);

            SaveMetadataToFile();
        }

        public void DeleteRowFromTable(string tableName, string key)
        {
            ThrowIfNoDatabaseInUse();

            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            var tableMetadata = GetTableFromCurrentDatabase(tableName);

            key = "#" + key;
            ThrowIfTableMetadataIsNull(tableMetadata, tableName);


            var primaryKey = tableMetadata.PrimaryKey.PrimaryKeyAttribute;
            ThrowIfPrimaryKeyIsNull(primaryKey);


            CheckForeignKeyConstraints(tableName, key);

            DeleteRowFromIndices(tableName, key);
            _dbAgent.DeleteFromStorage(tablePath, key);

            SaveMetadataToFile();
        }

        public string SelectFromTable(string tableName, List<string> columnList, string keyColumn, string keyValue)
        {
            ThrowIfNoDatabaseInUse();
            var resultTableRows = SelectRowFromTable(tableName, columnList, keyColumn, keyValue);
            return string.Join(" ", resultTableRows);

            //return GroupByHavingCount("studenti", "varsta", 2, "<");
            //return GroupByHavingSum("note", "nota", 7, ">");
        }

        public void CreateIndex(string indexName, string tableName, List<string> columnNames)
        {
            ThrowIfNoDatabaseInUse();

            var table = GetTableFromCurrentDatabase(tableName);

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

        public void AddForeignKey(string tableName, List<string> tableColumns, string referencedTableName, List<string> referencedTableColumns)
        {
            ThrowIfNoDatabaseInUse();

            var table = GetTableFromCurrentDatabase(tableName);
            var foreignKeyEntry = new ForeignKeyEntry(tableColumns, referencedTableName, referencedTableColumns);
            table.ForeignKeys.Add(foreignKeyEntry);
            SaveMetadataToFile();
        }

        public string IndexedNestedLoopJoin(string tableName1, string tableName2, string joinColumn1, string joinColumn2, List<string> columnNames)
        {
            ThrowIfNoDatabaseInUse();
            var table1 = GetTableFromCurrentDatabase(tableName1);
            var table2 = GetTableFromCurrentDatabase(tableName2);

            ThrowIfTableMetadataIsNull(table1, tableName1);
            ThrowIfTableMetadataIsNull(table2, tableName2);

            var result = new List<string>();

            if (table1.PrimaryKey.PrimaryKeyAttribute.Equals(joinColumn1))
            {
                result = SelectFromJoinedTables(table1, table2, joinColumn1, joinColumn2, columnNames);
            }
            if (table2.PrimaryKey.PrimaryKeyAttribute.Equals(joinColumn2))
            {
                result = SelectFromJoinedTables(table2, table1, joinColumn2, joinColumn1, columnNames);
            }

            if (result == null)
            {
                throw new Exception("Something is wrong");
            }

            return string.Join("    ", result);
        }

        public string HashJoin(string tableName1, string tableName2, string joinColumn1, string joinColumn2, List<string> columnNames)
        {
            ThrowIfNoDatabaseInUse();
            var table1 = GetTableFromCurrentDatabase(tableName1);
            var table2 = GetTableFromCurrentDatabase(tableName2);

            ThrowIfTableMetadataIsNull(table1, tableName1);
            ThrowIfTableMetadataIsNull(table2, tableName2);

            List<string> result = new List<string>();

            if (table1.PrimaryKey.PrimaryKeyAttribute.Equals(joinColumn1))
            {
                result = SelectFromHashJoinedTables(table1, table2, joinColumn1, joinColumn2, columnNames);
            }

            if (table2.PrimaryKey.PrimaryKeyAttribute.Equals(joinColumn2))
            {
                result = SelectFromHashJoinedTables(table2, table1, joinColumn2, joinColumn1, columnNames);
            }

            if (result == null)
            {
                throw new Exception("Something is wrong");
            }

            return string.Join("    ", result);
        }

        public string GroupByCount(string tableName, string groupByColumn)
        {
            ThrowIfNoDatabaseInUse();
            var table = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(table, tableName);

            var result = new List<string>();
            var groupBy = SelectGroupByForGivenColumn(table, groupByColumn);

            foreach(var group in groupBy)
            {
                result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
            }

            return string.Join("    ", result);
        }

        public string GroupBySum(string tableName, string groupByColumn)
        {
            ThrowIfNoDatabaseInUse();
            var table = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(table, tableName);

            var result = new List<string>();
            var groupBy = SelectGroupByForGivenColumn(table, groupByColumn);

            foreach (var group in groupBy)
            {
                result.Add(group.Split(':').FirstOrDefault() + " " + Sum(group, GetColumnIndex(table,  groupByColumn)));
            }

            return string.Join("    ", result);
        }

        public string GroupByHavingCount(string tableName, string groupByColumn, int value, string comparer)
        {
            ThrowIfNoDatabaseInUse();
            var table = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(table, tableName);

            var result = new List<string>();
            var groupBy = SelectGroupByForGivenColumn(table, groupByColumn);

            foreach(var group in groupBy)
            {
                if (comparer.Equals(">"))
                {
                    if (Count(group) > value)
                        result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
                }
                else if (comparer.Equals("<"))
                {
                    if (Count(group) < value)
                        result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
                }
                else if (comparer.Equals(">="))
                {
                    if (Count(group) >= value)
                        result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
                }
                else if (comparer.Equals("<="))
                {
                    if (Count(group) <= value)
                        result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
                }
                else if (comparer.Equals("="))
                {
                    if (Count(group) == value)
                        result.Add(group.Split(':').FirstOrDefault() + " " + Count(group));
                }
            }

            return string.Join("    ", result);
        }

        public string GroupByHavingSum(string tableName, string groupByColumn, int value, string comparer)
        {
            ThrowIfNoDatabaseInUse();
            var table = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(table, tableName);
            ThrowIfColumnIsNotInt(table, groupByColumn);

            List<string> result = new List<string>();

            var groupBy = SelectGroupByForGivenColumn(table, groupByColumn);
            int index = GetColumnIndex(table, groupByColumn);

            foreach (var group in groupBy)
            {
                if (comparer.Equals(">"))
                {
                    if (Sum(group, index) > value)
                        result.Add(group);
                }
                else if (comparer.Equals("<"))
                {
                    if (Sum(group, index) < value)
                        result.Add(group);
                }
                else if (comparer.Equals(">="))
                {
                    if (Sum(group, index) >= value)
                        result.Add(group);
                }
                else if (comparer.Equals("<="))
                {
                    if (Sum(group, index) <= value)
                        result.Add(group);
                }
                else if (comparer.Equals("="))
                {
                    if (Sum(group, index) == value)
                        result.Add(group);
                }
            }

            return string.Join("    ", result);
        }
        #endregion

        #region Helpers
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

        private void SaveMetadataToFile()
        {
            var serializedDatabaseMetadata = JsonConvert.SerializeObject(_databaseMetadata);
            File.WriteAllText(PathHelper.MetadataFilePath, serializedDatabaseMetadata);
        }

        private DatabaseMetadataEntry GetDatabase(string databaseName)
        {
            var matchedDatabase = _databaseMetadata.Databases.SingleOrDefault(database => database.DatabaseName.Equals(databaseName));
            ThrowIfDatabaseNotFound(matchedDatabase);
            return matchedDatabase;
        }

        private void CreateDirectoryForTable(string tableName)
        {
            var directory = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private TableMetadataEntry GetTableFromCurrentDatabase(string tableName)
        {
            var tableMetadata = _currentDatabase.Tables.SingleOrDefault(table => table.TableName.Equals(tableName));
            ThrowIfTableMetadataIsNull(tableMetadata, tableName);

            return tableMetadata;
        }

        private KeyValuePair<string, string> CreateKeyValueForData(List<string> columnNames, List<string> values, List<string> primaryKey)
        {
            var key = string.Empty;
            var valuesString = string.Empty;

            for (var i = 0; i < values.Count; i++)
            {
                if (primaryKey.Contains(columnNames[i]))
                {
                    key += "#" + values[i];
                }
                valuesString += "#" + values[i];
            }

            return new KeyValuePair<string, string>(key, valuesString);
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

        private List<ForeignKeyEntry> GetForeignKeysForTable(string tableName)
        {
            var table = GetTableFromCurrentDatabase(tableName);
            return table.ForeignKeys;
        }

        private bool KeyAlreadyStored(string tablePath, string key)
        {
            return _dbAgent.GetFromStorage(tablePath, key) != null;
        }

        private void UpdateIndices(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            foreach (var index in tableMetadata.IndexFiles)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, index.IndexName);
                var keyValueToInsert = CreateKeyValueForData(columnNames, values, index.IndexAttributes);
                var oldValue = _dbAgent.GetFromStorage(indexPath, keyValueToInsert.Key);
                if (oldValue == null)
                {
                    _dbAgent.InsertIntoStorage(indexPath, keyValueToInsert.Key, keyValueToInsert.Value);
                }
                else
                {
                    _dbAgent.DeleteFromStorage(indexPath, keyValueToInsert.Key);
                    _dbAgent.InsertIntoStorage(indexPath, keyValueToInsert.Key, oldValue + '|' + keyValueToInsert.Value);
                }
            }
        }

        private void UpdateUniqueIndices(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            foreach (var uniqueKey in tableMetadata.UniqueKeyEntry)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, uniqueKey.UniqueAttribute);
                var keyValueToInsert = CreateKeyValueForData(columnNames, values, uniqueKey.UniqueAttribute);
                _dbAgent.InsertIntoStorage(indexPath, keyValueToInsert.Key, keyValueToInsert.Value);
            }
        }

        private void UpdateForeignKeys(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            foreach (var entry in tableMetadata.ForeignKeys)
            {
                var key = string.Empty;
                var valuesString = string.Empty;

                for (var i = 0; i < values.Count; i++)
                {
                    if (entry.Columns.FirstOrDefault().Equals(columnNames[i]))
                    {
                        key = "#" + values[i];
                    }
                    valuesString += "#" + values[i];
                }

                var referencedTableStoragePath = PathHelper.GetReferencedTablePath(_currentDatabase.DatabaseName, tableMetadata.TableName, entry.ReferencedTableName);
                var oldValue = _dbAgent.GetFromStorage(referencedTableStoragePath, key);
                if (oldValue == null)
                {
                    _dbAgent.InsertIntoStorage(referencedTableStoragePath, key, valuesString);
                }
                else
                {
                    _dbAgent.DeleteFromStorage(referencedTableStoragePath, key);
                    _dbAgent.InsertIntoStorage(referencedTableStoragePath, key, oldValue + '|' + valuesString);
                }
            }
        }

        private void CheckForeignKeyConstraints(string tableName, string key)
        {
            foreach (var table in _currentDatabase.Tables)
            {
                ThrowIfKeyIsForeignKeyToAnotherTable(table.TableName, tableName, key);
            }
        }

        private void DeleteRowFromIndices(string tableName, string primaryKey)
        {
            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            var tableMetadata = GetTableFromCurrentDatabase(tableName);

            var row = _dbAgent.GetFromStorage(tablePath, primaryKey);
            var values = row.Split('#');

            foreach (var index in tableMetadata.IndexFiles)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableName, index.IndexName);
                string key = string.Empty;
                for (int i = 1; i < values.Length; i++)
                {
                    if (index.IndexAttributes.Contains(tableMetadata.Structure[i - 1].AttributeName))
                    {
                        key += "#" + values[i];
                    }
                }

                _dbAgent.DeleteFromStorage(indexPath, key);
            }

            foreach (var uniqueKey in tableMetadata.UniqueKeyEntry)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableName, uniqueKey.UniqueAttribute);
                string key = string.Empty;
                for (int i = 1; i < values.Length; i++)
                {
                    if (uniqueKey.UniqueAttribute.Equals(tableMetadata.Structure[i - 1].AttributeName))
                    {
                        key += "#" + values[i];
                    }
                }

                _dbAgent.DeleteFromStorage(indexPath, key);
            }

        }

        private List<string> SelectRowFromTable(string tableName, List<string> columnNames, string searchedKeyColumn, string searchedKeyValue, bool foreignKeyCheck = false)
        {
            var tableMetadata = GetTableFromCurrentDatabase(tableName);
            ThrowIfTableMetadataIsNull(tableMetadata, tableName);

            var key = "#" + searchedKeyValue;
            string path = string.Empty;

            if (searchedKeyColumn == null)
            {
                return SelectAllFromTable(tableMetadata, columnNames);
            }

            if (searchedKeyColumn.Equals(tableMetadata.PrimaryKey.PrimaryKeyAttribute) || foreignKeyCheck == true)
            {
                path = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableName);
            }
            else
            {
                foreach (var index in tableMetadata.IndexFiles)
                {
                    if (index.IndexAttributes.Contains(searchedKeyColumn))
                    {
                        path = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableName, index.IndexName);
                    }
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                foreach (var uniqueKey in tableMetadata.UniqueKeyEntry)
                {
                    if (uniqueKey.UniqueAttribute.Equals(searchedKeyColumn))
                    {
                        path = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableName, searchedKeyColumn);
                    }
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return SelectAllFromTableWhere(tableMetadata, columnNames, searchedKeyColumn, searchedKeyValue);
            }

            var resultRow = _dbAgent.GetFromStorage(path, key);

            return resultRow != null ? FormatResultRow(resultRow, columnNames, tableMetadata) : new List<string>();
        }

        private List<string> SelectAllFromTableWhere(TableMetadataEntry tableMetadata, List<string> columnNames, string searchedKeyColumn, string searchedKeyValue)
        {
            var path = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableMetadata.TableName);
            List<KeyValuePair<string, string>> resultList = _dbAgent.GetAllFromStorage(path);
            int index = -1;
            var result = string.Empty;

            foreach (var attr in tableMetadata.Structure)
            {
                if (attr.AttributeName.Equals(searchedKeyColumn))
                {
                    index = tableMetadata.Structure.IndexOf(attr);
                }
            }

            if (index == -1)
            {
                throw new Exception("Column does not exist");
            }

            foreach (var row in resultList)
            {
                var values = row.Value.Split('#');
                if (values[index + 1].Equals(searchedKeyValue))
                {
                    result += row.Value + '|';
                }
            }

            return FormatResultRow(result, columnNames, tableMetadata);
        }

        private List<string> FormatResultRow(string row, List<string> columnNames, TableMetadataEntry tableMetadata)
        {
            var count = Count(row);
            var result = new List<string>();
            var rows = row.Split('|');
            foreach (string newRow in rows)
            {
                var values = newRow.Split('#');
                for (var i = 0; i < values.Length; i++)
                {
                    if (string.IsNullOrEmpty(values[i].Trim()))
                    {
                        continue;
                    }
                    if (columnNames == null || columnNames.Contains(tableMetadata.Structure.ElementAt(i).AttributeName) || columnNames.Count == 0)
                    {
                        result.Add(values[i]);
                    }
                }

                result.Add(" | ");
            }

            return result;
        }

        private List<string> SelectAllFromTable(TableMetadataEntry tableMetadata, List<string> columnNames)
        {
            var path = PathHelper.GetTablePath(_currentDatabase.DatabaseName, tableMetadata.TableName);
            List<KeyValuePair<string, string>> resultList = _dbAgent.GetAllFromStorage(path);
            var result = string.Empty;

            for (int i = 0; i < resultList.Count; i++)
            {
                result += resultList[i].Value + '|';
            }

            return FormatResultRow(result, columnNames, tableMetadata);
        }

        private List<string> SelectFromJoinedTables(TableMetadataEntry table1, TableMetadataEntry table2, string joinColumn1, string joinColumn2, List<string> columnNames)
        {
            List<string> result = new List<string>();

            foreach (var foreignKey in table2.ForeignKeys)
            {
                if (foreignKey.Columns.Contains(joinColumn2))
                {
                    var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, table1.TableName);
                    var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, table2.TableName, table1.TableName);
                    var table1Data = _dbAgent.GetAllFromStorage(tablePath);

                    foreach (var entry in table1Data)
                    {
                        var key = entry.Key;
                        var value = _dbAgent.GetFromStorage(indexPath, key);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (!value.Contains('|'))
                            {
                                result.Add(string.Join(" ", FormatResultRow(entry.Value + value, columnNames, table1)));
                            }
                            else
                            {
                                var values = value.Split('|');
                                foreach (var newValue in values)
                                {
                                    result.Add(string.Join(" ", FormatResultRow(entry.Value + newValue, columnNames, table1)));
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<string> SelectFromHashJoinedTables(TableMetadataEntry table1, TableMetadataEntry table2, string joinColumn1, string joinColumn2, List<string> columnNames)
        {
            List<string> result = new List<string>();
            var hashTable1 = GetHashTableForRelation(table1);

            foreach (var foreignKey in table2.ForeignKeys)
            {
                if (foreignKey.Columns.Contains(joinColumn2))
                {
                    var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, table2.TableName, table1.TableName);
                    var table2Data = _dbAgent.GetAllFromStorage(indexPath);

                    foreach (var entry in table2Data)
                    {
                        var key = entry.Key;
                        string value = string.Empty;
                        hashTable1.TryGetValue(key, out value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (!entry.Value.Contains('|'))
                            {
                                result.Add(string.Join(" ", FormatResultRow(value + entry.Value, columnNames, table1)));
                            }
                            else
                            {
                                var values = entry.Value.Split('|');
                                foreach (var newValue in values)
                                {
                                    result.Add(string.Join(" ", FormatResultRow(value + newValue, columnNames, table1)));
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<string> SelectGroupBy(TableMetadataEntry table, string path)
        {
            List<KeyValuePair<string, string>> resultList = _dbAgent.GetAllFromStorage(path);
            List<string> result = new List<string>();

            foreach (var row in resultList)
            {
                result.Add(row.Key + " : " + string.Join(" ", FormatResultRow(row.Value, null, table)));
            }

            return result;
        }

        private List<string> SelectGroupByForGivenColumn(TableMetadataEntry table, string groupByColumn)
        {
            if (table.PrimaryKey.PrimaryKeyAttribute.Equals(groupByColumn))
            {
                return SelectGroupBy(table, PathHelper.GetTablePath(_currentDatabase.DatabaseName, table.TableName));
            }

            foreach (var uniqueKey in table.UniqueKeyEntry)
            {
                if (uniqueKey.UniqueAttribute.Equals(groupByColumn))
                {
                    return SelectGroupBy(table, PathHelper.GetIndexPath(_currentDatabase.DatabaseName, table.TableName, uniqueKey.UniqueAttribute));
                }
            }

            foreach (var index in table.IndexFiles)
            {
                if (index.IndexAttributes.Contains(groupByColumn))
                {
                    return SelectGroupBy(table, PathHelper.GetIndexPath(_currentDatabase.DatabaseName, table.TableName, index.IndexName));
                }
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                if (foreignKey.Columns.Contains(groupByColumn))
                {
                    return SelectGroupBy(table, PathHelper.GetIndexPath(_currentDatabase.DatabaseName, table.TableName, foreignKey.ReferencedTableName));
                }
            }

            var dictionary = GetHashTableForGivenColumn(table, groupByColumn);
            List<string> result = new List<string>();

            foreach(var row in dictionary)
            {
                string values = string.Empty;
                foreach(var value in row.Value)
                {
                    values += value + '|';
                }

                result.Add(row.Key + " : " + string.Join(" ", FormatResultRow(values, null, table)));
            }

            return result;
        }

        private Dictionary<string, string> GetHashTableForRelation(TableMetadataEntry table)
        {
            var hashTable = new Dictionary<string, string>();
            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, table.TableName);
            var rows = _dbAgent.GetAllFromStorage(tablePath);

            foreach (var row in rows)
            {
                hashTable.Add(row.Key, row.Value);
            }

            return hashTable;
        }

        private Dictionary<string, List<string>> GetHashTableForGivenColumn(TableMetadataEntry table, string columnName)
        {
            var hashTable = new Dictionary<string, List<string>>();
            var tablePath = PathHelper.GetTablePath(_currentDatabase.DatabaseName, table.TableName);
            var rows = _dbAgent.GetAllFromStorage(tablePath);
            var index = -1;

            foreach(var column in table.Structure)
            {
                if (column.AttributeName.Equals(columnName))
                {
                    index = table.Structure.IndexOf(column);
                }
            }

            if (index == -1)
                throw new Exception("Column does not exist");

            foreach (var row in rows)
            {
                var values = row.Value.Split('#');
                if(hashTable.ContainsKey(values[index + 1]))
                {
                    hashTable[values[index + 1]].Add(row.Value);
                }
                else
                {
                    hashTable.Add(values[index + 1], new List<string> { row.Value });
                }
            }

            return hashTable;
        }

        private int Count(string row)
        {
            int count = 0;
            var values = row.Split(':').LastOrDefault().Split('|');

            foreach(var value in values)
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                    count++;
            }

            return count;
        }

        private int Sum(string row, int columnIndex)
        {
            int sum = 0;
            var values = row.Split(':').LastOrDefault().Split('|');

            foreach (var value in values)
            {
                string newValue = value.Trim();
                if (!string.IsNullOrEmpty(newValue))
                {
                    var columns = newValue.Split(' ');
                    sum +=Int32.Parse(columns[columnIndex]);
                }
            }

            return sum;
        }

        private int GetColumnIndex(TableMetadataEntry table, string columnName)
        {
            int index = -1;
            foreach (var column in table.Structure)
            {
                if (column.AttributeName.Equals(columnName))
                {
                    index = table.Structure.IndexOf(column);
                }
            }

            return index;
        }
        #endregion

        #region Throw
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

        private void ThrowIfIndexConstraintsAreNotMet(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            foreach (var index in tableMetadata.IndexFiles)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, index.IndexName);
                var keyValueToInsert = CreateKeyValueForData(columnNames, values, index.IndexAttributes);
                ThrowIfKeyAlreadyStored(indexPath, keyValueToInsert.Key);
            }
        }

        private void ThrowIfUniqueConstraintsAreNotMet(TableMetadataEntry tableMetadata, List<string> values, List<string> columnNames)
        {
            foreach (var uniqueKey in tableMetadata.UniqueKeyEntry)
            {
                var indexPath = PathHelper.GetIndexPath(_currentDatabase.DatabaseName, tableMetadata.TableName, uniqueKey.UniqueAttribute);
                var keyValueToInsert = CreateKeyValueForData(columnNames, values, uniqueKey.UniqueAttribute);
                ThrowIfKeyAlreadyStored(indexPath, keyValueToInsert.Key);
            }
        }

        private void ThrowIfKeyIsForeignKeyToAnotherTable(string tableName, string referencedTable, string key)
        {
            var indexPath = PathHelper.GetReferencedTablePath(_currentDatabase.DatabaseName, tableName, referencedTable);
            if (KeyAlreadyStored(indexPath, key))
            {
                throw new KeyCouldNotBeDeletedException(indexPath, key);
            }
        }

        private void ThrowIfForeignKeyConstraintsAreNotMet(TableMetadataEntry tableMetadata, List<string> columnNames, List<string> values)
        {
            foreach (var entry in tableMetadata.ForeignKeys)
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (entry.Columns.FirstOrDefault().Equals(columnNames[i]))
                    {
                        if (SelectRowFromTable(entry.ReferencedTableName, null, columnNames[i], values[i], true).Count == 0)
                        {
                            // This is called after everything else is inserted, throwing here would leave the database in a bad state
                            // Since this is part of the row add and it checks constraints, it should be called the first.
                            throw new Exception("Key does not exist");
                        }
                    }
                }
            }
        }

        private void ThrowIfKeyAlreadyStored(string tablePath, string key)
        {
            if (KeyAlreadyStored(tablePath, key))
            {
                throw new KeyAlreadyStoredException(tablePath, key);
            }
        }

        private void ThrowIfPrimaryKeyIsNull(string primaryKey)
        {
            if (primaryKey == null)
            {
                throw new InsertIntoCommandColumnCountDoesNotMatchValueCount();
            }
        }

        private void ThrowIfTableMetadataIsNull(TableMetadataEntry tableMetadata, string tableName)
        {
            if (tableMetadata == null)
            {
                throw new TableDoesNotExistException(_currentDatabase.DatabaseName, tableName);
            }
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
            //When creating a table right after the database, _currentDatabase.Tables is null, resulting in an exception
            if (_currentDatabase.Tables != null)
            {
                var tableAlreadyExists = _currentDatabase.Tables.Any(table => table.TableName.Equals(tableName));
                if (tableAlreadyExists)
                {
                    throw new TableAlreadyExistsException(tableName);
                }
            }
        }

        private void ThrowIfDatabaseNotFound(DatabaseMetadataEntry databaseMetadata)
        {
            if (databaseMetadata == null)
            {
                throw new DataBaseDoesNotExistException();
            }
        }

        private void ThrowIfColumnIsNotInt(TableMetadataEntry tableMetadata, string columnName)
        {
                if(!tableMetadata.Structure[GetColumnIndex(tableMetadata, columnName)].Type.Equals("int"))
                {
                    throw new Exception("Only int can be summed");
                }
        }

        #endregion
    }
}
