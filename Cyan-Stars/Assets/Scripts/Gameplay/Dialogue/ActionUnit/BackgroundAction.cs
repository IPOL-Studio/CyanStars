using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("Background")]
    public class BackgroundAction : BaseActionUnit
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        //Effect
        //Time

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(EventConst.SetBackgroundEvent, this, SingleEventArgs<string>.Create(FilePath));
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnComplete()
        {
            IsCompleted = true;
        }
    }
}
