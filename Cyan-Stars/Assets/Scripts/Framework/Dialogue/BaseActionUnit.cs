using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseActionUnit
    {
        public abstract Task ExecuteAsync();
    }
}
