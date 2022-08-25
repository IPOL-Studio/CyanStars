using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Action")]
    public class ActionNode : BaseFlowNode
    {
        [JsonProperty("actions")]
        public List<BaseActionUnit> Actions { get; set; }

        public override void OnInit()
        {
            int index = 0;
            while (index < Actions.Count)
            {
                Actions[index].OnInit();
                index = CheckActionIsCompleted(index);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (CheckCompleted())
                return;

            int index = 0;
            while (index < Actions.Count)
            {
                Actions[index].OnUpdate(deltaTime);
                index = CheckActionIsCompleted(index);
            }

            CheckCompleted();
        }

        public override void OnComplete()
        {
            foreach (BaseActionUnit action in Actions)
            {
                if (!action.IsCompleted)
                {
                    action.OnComplete();
                }
            }
        }

        private bool CheckCompleted()
        {
            IsCompleted = Actions.Count == 0;
            return IsCompleted;
        }

        private int CheckActionIsCompleted(int index)
        {
            if (Actions[index].IsCompleted)
            {
                Actions.RemoveAt(index);
            }
            else
            {
                index++;
            }

            return index;
        }
    }
}
