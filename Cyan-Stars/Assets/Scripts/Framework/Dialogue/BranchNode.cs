using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Branch")]
    public class BranchNode : BaseNode
    {
        [JsonProperty("options")]
        public List<BranchOption> Options { get; set; }

        public override void OnInit()
        {
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (BranchOption option in Options)
            {
                if (option.IsSelected)
                {
                    NextNodeID = option.NextNodeID;
                    IsCompleted = true;
                    return;
                }
            }
        }
    }
}
