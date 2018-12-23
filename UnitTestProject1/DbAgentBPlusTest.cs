using KeyValueDatabaseApi.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyValueDatabaseApiUnitTests
{
    [TestClass]
    public class DbAgentBPlusTest
    {
        [TestMethod]
        public void BasicDBAgentBPlusTest()
        {
            const string storageFile = "testStorage";
            var dbAgentBPlus = new DbAgentBPlus();

            var key = "key1";
            var value = "value1";
            dbAgentBPlus.InsertIntoStorage(storageFile, key, value);

            var retrievedValue = dbAgentBPlus.GetFromStorage(storageFile, key);
            Assert.AreEqual(value, retrievedValue);

            var deletedValue = dbAgentBPlus.DeleteFromStorage(storageFile, key);
            Assert.AreEqual(retrievedValue, deletedValue);

            dbAgentBPlus.ClearStorage(storageFile);
        }
    }
}
