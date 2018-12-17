using System;

namespace KeyValueDatabaseApi.Context
{
    interface IDbAgent
    {
        bool InsertIntoStorage(string storagePath, string key, string value);

        string GetFromStorage(string storagePath, string key);

        string DeleteFromStorage(string storagePath, string key);

        void ClearStorage(string storagePath);
    }
}
