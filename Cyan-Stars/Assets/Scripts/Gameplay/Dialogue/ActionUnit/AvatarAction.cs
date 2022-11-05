using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("SetAvatar")]
    public class AvatarAction : BaseActionUnit
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        public override Task ExecuteAsync()
        {
            return GameRoot.Dialogue.GetService<CharacterInfoManager>().SetAvatar(FilePath);
        }
    }
}
