namespace KeyValueDatabaseApi.Context
{
    interface IDbAgent
    {
        void InsertIntoStorage(string tablePath, string key, string value);

        string GetFromStorage(string storagePath, string key);

        void DeleteFromIndexFile(string storagePath, string key, string storageName);
    }
}
