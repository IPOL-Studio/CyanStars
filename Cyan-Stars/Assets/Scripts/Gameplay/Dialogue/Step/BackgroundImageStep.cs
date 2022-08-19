using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueStep("SetBackgroundImage")]
    public class BackgroundImageStep : BaseStep
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(EventConst.SetBackgroundImageEvent, this, SingleEventArgs<string>.Create(FilePath));
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }
    }
}
