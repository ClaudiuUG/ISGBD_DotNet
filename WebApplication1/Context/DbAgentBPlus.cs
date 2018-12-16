using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace KeyValueDatabaseApi.Context
{
    public class DbAgentBPlus : IDbAgent
    {
        public void InsertIntoStorage(string tablePath, string key, string value)
        {
            var options = new BPlusTree<string, string>.OptionsV2(PrimitiveSerializer.String, PrimitiveSerializer.String);
            options.CalcBTreeOrder(16, 24);
            options.CreateFile = CreatePolicy.Always;
            options.FileName = tablePath;
            using (var tree = new BPlusTree<string, string>(options))
            {
                tree.Add(key, value);
            }

            // nothing else to implement in this method, but it's still work in progress
            throw new System.NotImplementedException();
        }

        public string GetFromStorage(string storagePath, string key)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteFromIndexFile(string storagePath, string key, string storageName)
        {
            throw new System.NotImplementedException();
        }
    }
}