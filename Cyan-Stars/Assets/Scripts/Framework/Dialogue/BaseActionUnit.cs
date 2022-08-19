using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseActionUnit
    {
        [JsonIgnore]
        public bool IsCompleted { get; protected set; }

        public abstract void OnInit();
        public abstract void OnUpdate(float deltaTime);
    }
}
