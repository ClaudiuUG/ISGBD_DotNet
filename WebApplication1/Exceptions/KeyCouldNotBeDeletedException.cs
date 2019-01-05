using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeyValueDatabaseApi.Exceptions
{
    public class KeyCouldNotBeDeletedException : Exception
    {
        public KeyCouldNotBeDeletedException(string tablePath, string key)
            : base($"Table {tablePath} has entries with the foreign key {key}")
        { }
    }
}