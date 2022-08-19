using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueStep("SetName")]
    public class NameStep : BaseStep
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(EventConst.SetNameTextEvent, this, SingleEventArgs<string>.Create(Text));
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }
    }
}
