using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using UnityEngine;

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

        public override Task ExecuteAsync()
        {
            CheckValues();

            float fadeIn = Mathf.Clamp(FadeInTime, 0, float.MaxValue);
            float fadeOut = Mathf.Clamp(FadeOutTime, 0, float.MaxValue);

            var args = new PlayMusicArgs
            {
                FilePath = FilePath, FadeInTime = fadeIn, FadeOutTime = fadeOut, IsCrossFading = IsCrossFading
            };

            GameRoot.Dialogue.GetService<AudioManager>().PlayMusic(args);
            return Task.CompletedTask;
        }

        private void CheckValues()
        {
            if (FadeInTime < 0)
            {
                Debug.LogError("fade in time should equal or great 0");
                Debug.LogWarning("will set fade in time to 0");
            }

            if (FadeOutTime < 0)
            {
                Debug.LogError("fade out time should equal or great 0");
                Debug.LogWarning("will set fade out time to 0");
            }
        }
    }
}
