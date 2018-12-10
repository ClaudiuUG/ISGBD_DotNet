using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;

namespace KeyValueDatabaseApi.Context
{
    [Serializable]
    public class Metadata
    {
        public Metadata(List<DatabaseMetadataEntry> databases)
        {
            Databases = databases;
        }

        public List<DatabaseMetadataEntry> Databases { get; set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Databases == null)
            {
                Databases = new List<DatabaseMetadataEntry>();
            }
        }
    }

    [Serializable]
    public class DatabaseMetadataEntry
    {
        public DatabaseMetadataEntry(List<TableMetadataEntry> tables, string databaseName)
        {
            Tables = tables;
            DatabaseName = databaseName;
        }

        public List<TableMetadataEntry> Tables { get; set; }

        public string DatabaseName { get; set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Tables == null)
            {
                Tables = new List<TableMetadataEntry>();
            }
        }
    }

    [Serializable]
    public class TableMetadataEntry
    {
        public TableMetadataEntry(
            string tableName,
            List<AttributeEntry> structure,
            PrimaryKeyEntry primaryKey,
            List<UniqueKeyEntry> uniqueKeys,
            List<IndexFileEntry> indexFiles)
        {
            TableName = tableName;
            Structure = structure;
            PrimaryKey = primaryKey;
            IndexFiles = indexFiles;
        }

        public string TableName { get; set; }

        public List<AttributeEntry> Structure { get; set; }

        public PrimaryKeyEntry PrimaryKey { get; set; }

        public List<UniqueKeyEntry> UniqueKeyEntry { get; set; }

        public List<IndexFileEntry> IndexFiles { get; set; }

        public List<ForeignKeyEntry> ForeignKeys { get; set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Structure == null)
            {
                Structure = new List<AttributeEntry>();
            }
            if (UniqueKeyEntry == null)
            {
                UniqueKeyEntry = new List<UniqueKeyEntry>();
            }
            if (IndexFiles == null)
            {
                IndexFiles = new List<IndexFileEntry>();
            }
        }
    }

    [Serializable]
    public class AttributeEntry
    {
        public AttributeEntry(string attributeName, string type, int length, bool isNull)
        {
            AttributeName = attributeName;
            Type = type;
            Length = length;
            IsNull = isNull;
        }

        public string AttributeName { get; set; }

        public string Type { get; set; }

        public int Length { get; set; }

        public bool IsNull { get; set; }
    }

    [Serializable]
    public class PrimaryKeyEntry
    {
        public PrimaryKeyEntry(string primaryKeyAttribute)
        {
            PrimaryKeyAttribute = primaryKeyAttribute;
        }

        public string PrimaryKeyAttribute { get; set; }
    }

    [Serializable]
    public class UniqueKeyEntry
    {
        public UniqueKeyEntry(string uniqueAttribute)
        {
            UniqueAttribute = uniqueAttribute;
        }

        public string UniqueAttribute { get; set; }
    }

    [Serializable]
    public class IndexFileEntry
    {
        public IndexFileEntry(string indexName, List<string> indexAttributes)
        {
            IndexName = indexName;
            IndexAttributes = indexAttributes;
        }

        public string IndexName { get; set; }

        public List<string> IndexAttributes { get; set; }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (IndexAttributes == null)
            {
                IndexAttributes = new List<string>();
            }
        }
    }

    [Serializable]
    public class ForeignKeyEntry
    {
        public ForeignKeyEntry(List<string> columns, string referencedTableName, List<string> referencedTableColumns)
        {
            
        }

        public List<string> Columns { get; set; }

        public string ReferencedTableName { get; set; }

        public List<string> ReferencedTableColumns { get; set; }

        [OnSerialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Columns == null)
            {
                Columns = new List<string>();
            }

            if (ReferencedTableColumns == null)
            {
                ReferencedTableColumns = new List<string>();
            }
        }
    }
}