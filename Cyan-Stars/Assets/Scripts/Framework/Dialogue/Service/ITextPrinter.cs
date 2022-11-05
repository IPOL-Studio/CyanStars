using System.Threading.Tasks;

namespace CyanStars.Framework.Dialogue
{
    public interface ITextPrinter : IService
    {
        Task PrintTextAsync(string text, bool isAppend, int? stop = null);
    }
}
