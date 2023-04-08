using System.Threading.Tasks;

namespace CyanStars.Framework.Dialogue
{
    public interface IScriptExecutorOperationHandler
    {
        Task OnPause();
    }
}
