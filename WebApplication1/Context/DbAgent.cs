using DataTanker;
using DataTanker.Settings;
using KeyValueDatabaseApi.Exceptions;
namespace KeyValueDatabaseApi.Context
{
    internal class DbAgent : IDbAgent
    {
        private StorageFactory _storageFactory = new StorageFactory();

        public void InsertIntoStorage(string tablePath, string key, string value)
        {
            using (var storage = GetStorage())
            {
                storage.OpenOrCreate(tablePath);
                if (storage.Exists(key))
                {
                    throw new EntryAlreadyExistsForKeyException(key);
                }
                storage.Set(key, value);
            }
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

        public void DeleteFromIndexFile(string storagePath, string key, string storageName)
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
        }
    }
}