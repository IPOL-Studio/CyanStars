using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
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

        public override Task ExecuteAsync()
        {
            return GameRoot.Dialogue.GetService<BackgroundManager>().SetBackground(FilePath);
        }
    }
}
