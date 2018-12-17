using System.IO;
using KeyValueDatabaseApi.Context;
using Xunit;

namespace XUnitTestProject1
{
    public class DbAgentBPlusTest
    {
        [Fact]
        public void InsertIntoStorageTest()
        {
            var dbAgentBPlus = new DbAgentBPlus();

            dbAgentBPlus.InsertIntoStorage($"testStorage", "key1", "value1");

            Directory.Exists($"testStorage");
        }
    }
}
