using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;

namespace KeyValueDatabaseApi.Context
{
    public class BPlusFactory
    {
        internal BPlusTree<T1, T2> CreateStoredBPlusForWrite<T1, T2>(string tablePath, ISerializer<T1> keySerializer, ISerializer<T2> valueSerializer)
        {
            var options = new BPlusTree<T1, T2>.OptionsV2(keySerializer, valueSerializer);
            options.CalcBTreeOrder(8, 64);
            options.CreateFile = CreatePolicy.IfNeeded;
            options.FileName = tablePath;
            return new BPlusTree<T1, T2>(options);
        }

        internal BPlusTree<T1, T2> CreateStoredBPlusForReadOrDelete<T1, T2>(string tablePath, ISerializer<T1> keySerializer,
            ISerializer<T2> valueSerializer)
        {
            var options = new BPlusTree<T1, T2>.OptionsV2(keySerializer, valueSerializer);
            options.CalcBTreeOrder(8, 64);
            options.CreateFile = CreatePolicy.IfNeeded;
            options.FileName = tablePath;
            return new BPlusTree<T1, T2>(options);
        }
    }
}