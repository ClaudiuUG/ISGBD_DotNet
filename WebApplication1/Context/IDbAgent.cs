using System;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Context
{
    interface IDbAgent
    {
        bool InsertIntoStorage(string storagePath, string key, string value);

        string GetFromStorage(string storagePath, string key);

        List<KeyValuePair<string, string>> GetAllFromStorage(string storagePath);

        string DeleteFromStorage(string storagePath, string key);

        void ClearStorage(string storagePath);
    }
}
