using System.Threading.Tasks;

namespace MunNovel.Service
{
    public interface ITextPrinter : IService
    {
        void SetSpeed(int value);
        Task Print(string text, bool isAppend = false);
    }
}
