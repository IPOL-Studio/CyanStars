using System.Threading.Tasks;

namespace MunNovel.Service
{
    public interface ITextPrinter
    {
        void SetSpeed(int value);
        ValueTask Print(string text, bool isAppend = false);
    }
}