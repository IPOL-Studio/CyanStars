using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MunNovel.Service
{
    public interface IChooser : IService
    {
        Task ShowOptionAsync(string text, string selectId, Action onSelect);
        Task WaitSelectComplete();
    }
}
