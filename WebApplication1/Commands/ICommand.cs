using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public interface ICommand
    {
        void Execute();
    }
}