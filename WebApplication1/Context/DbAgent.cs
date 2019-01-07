using System;
using System.Collections.Generic;
using DataTanker;
using DataTanker.Settings;
using KeyValueDatabaseApi.Exceptions;
namespace KeyValueDatabaseApi.Context
{
    [Obsolete("Use DbAgentBPlus")]
    internal class DbAgent : IDbAgent
    {
        private StorageFactory _storageFactory = new StorageFactory();

        public bool InsertIntoStorage(string storagePath, string key, string value)
        {
            using (var storage = GetStorage())
            {
                storage.OpenOrCreate(storagePath);
                if (storage.Exists(key))
                {
                    throw new EntryAlreadyExistsForKeyException(key);
                }
                storage.Set(key, value);
            }

            return true;
        }

        private IKeyValueStorage<ComparableKeyOf<string>, ValueOf<string>> GetStorage()
        {
            return _storageFactory.CreateBPlusTreeStorage<string, string>(BPlusTreeStorageSettings.Default(255));
        }

        public string GetFromStorage(string storagePath, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            var result = string.Empty;
            using (var storage = GetStorage())
            {
                storage.OpenOrCreate(storagePath);
                if (!storage.Exists(key))
                {
                    return string.Empty;
                }
                result = storage.Get(key).Value;
            }

            return result;
        }

        public string DeleteFromStorage(string storagePath, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new NullOrEmptyKeyException(key);
            }
            var result = string.Empty;
            using (var storage = GetStorage())
            {
                storage.OpenOrCreate(storagePath);
                if (!storage.Exists(key))
                {
                    throw new CouldNotFindKeyException(key);
                }
                storage.Remove(key);
            }

            return string.Empty;
        }

        public void ClearStorage(string storagePath)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, string>> GetAllFromStorage(string storagePath)
        {
            throw new NotImplementedException();
        }
    }
}