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
            GameRoot.Event.Dispatch(CreateBranchOptionsEventArgs.EventName, this, CreateBranchOptionsEventArgs.Create(Options));
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

        public override string ToString()
        {
            return $"ID: {ID}, Option count: {Options.Count}";
        }
    }
}
