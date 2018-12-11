using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeyValueDatabaseApi.Exceptions
{
    public class EntryAlreadyExistsForKeyException : Exception
    {
        public EntryAlreadyExistsForKeyException(string key) : base("Entry already exists for key: " + key)
        {
        }
    }
}