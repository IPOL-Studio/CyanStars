using System;
using System.Collections.Generic;

namespace MunNovel.Command
{
    public interface ICommandCreator
    {
        ICommand Create(Type commandType, Dictionary<string, object> param);
    }
}