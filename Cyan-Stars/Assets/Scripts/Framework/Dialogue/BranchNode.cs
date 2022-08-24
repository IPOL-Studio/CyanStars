using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Branch")]
    public class BranchNode : BaseFlowNode
    {
        [JsonProperty("options")]
        public List<BranchOption> Options { get; set; }

        public override void OnInit()
        {
            //TODO: 实现BranchNode
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

        public override void OnComplete()
        {

        }
    }
}
