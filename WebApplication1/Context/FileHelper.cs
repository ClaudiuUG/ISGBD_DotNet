using System;
using System.IO;

namespace KeyValueDatabaseApi.Context
{
    public static class FileHelper
    {
        public static void CreateDirectoryIfNotAlreadyExisting(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static bool CheckFileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}