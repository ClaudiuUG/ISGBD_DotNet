﻿using CSharpTest.Net.Serialization;

namespace KeyValueDatabaseApi.Context
{
    public class DbAgentBPlus : IDbAgent
    {
        private BPlusFactory _bPlusFactory = new BPlusFactory();

        public bool InsertIntoStorage(string storagePath, string key, string value)
        {
            var stringSerializer = PrimitiveSerializer.String;
            bool insertSuccessful;
            using (var tree = _bPlusFactory.CreateStoredBPlusForWrite(storagePath, stringSerializer, stringSerializer))
            {
                insertSuccessful = tree.TryAdd(key, value);
            }

            return insertSuccessful;
        }

        public string GetFromStorage(string storagePath, string key)
        {
            var stringSerializer = PrimitiveSerializer.String;
            string storedValue = null;
            using (var tree = _bPlusFactory.CreateStoredBPlusForReadOrDelete(storagePath, stringSerializer, stringSerializer))
            {
                tree.TryGetValue(key, out storedValue);
            }

            return storedValue;
        }

        public string DeleteFromStorage(string storagePath, string key)
        {
            var stringSerializer = PrimitiveSerializer.String;
            string storedValue = null;
            using (var tree = _bPlusFactory.CreateStoredBPlusForReadOrDelete(storagePath, stringSerializer, stringSerializer))
            {
                tree.TryRemove(key, out storedValue);
            }

            return storedValue;
        }

        public void ClearStorage(string storagePath)
        {
            var stringSerializer = PrimitiveSerializer.String;
            using (var tree = _bPlusFactory.CreateStoredBPlusForReadOrDelete(storagePath, stringSerializer, stringSerializer))
            {
                tree.Clear();
            }
        }
    }
}