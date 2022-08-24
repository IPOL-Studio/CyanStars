using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("PlayMusic")]
    public class PlayMusicAction : BaseActionUnit
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("fadeInTime")]
        public float FadeInTime { get; set; }

        [JsonProperty("fadeOutTime")]
        public float FadeOutTime { get; set; }

        [JsonProperty("isCrossFading")]
        public bool IsCrossFading { get; set; }

        public override void OnInit()
        {
            GameRoot.Event.Dispatch(PlayMusicEventArgs.EventName, this,
                PlayMusicEventArgs.Create(FilePath, FadeInTime, FadeOutTime, IsCrossFading));

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
