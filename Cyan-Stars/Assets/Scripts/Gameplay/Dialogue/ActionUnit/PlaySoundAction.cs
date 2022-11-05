using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("PlaySound", AllowMultiple = true)]
    public class PlaySoundAction : BaseActionUnit
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        public override Task ExecuteAsync()
        {
            GameRoot.Dialogue.GetService<AudioManager>().PlaySound(FilePath);
            return Task.CompletedTask;
        }
    }
}
