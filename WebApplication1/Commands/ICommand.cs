using DataTanker;
using System.Collections.Generic;

namespace KeyValueDatabaseApi.Commands
{
    public interface ICommand
    {
        string Execute();
    }
}