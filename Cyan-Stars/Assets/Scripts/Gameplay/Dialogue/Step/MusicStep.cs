using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueStep("Music")]
    public class MusicStep : BaseStep
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("fadeInTime")]
        public float FadeInTime { get; set; }

        [JsonProperty("fadeOutTime")]
        public float FadeOutTime { get; set; }

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(SetMusicEventArgs.EventName, this,
                SetMusicEventArgs.Create(FilePath, FadeInTime, FadeOutTime));

            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }
    }
}
