using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.Service
{
    public interface ICommandFactor : IService
    {
        ICommand CreateCommand(string commandName, Dictionary<string, object> param);

        ICommand CreateCommand(Type commandType, Dictionary<string, object> param);
    }
}
