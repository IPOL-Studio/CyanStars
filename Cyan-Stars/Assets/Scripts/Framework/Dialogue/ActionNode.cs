using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Action")]
    public class ActionNode : BaseNode
    {
        [JsonProperty("steps")]
        public List<BaseStep> Steps { get; set; }

        public override void OnInit()
        {
            int index = 0;
            while (index < Steps.Count)
            {
                Steps[index].OnInit();
                index = CheckStepIsCompleted(index);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (CheckCompleted())
                return;

            int index = 0;
            while (index < Steps.Count)
            {
                Steps[index].OnUpdate(deltaTime);
                index = CheckStepIsCompleted(index);
            }

            CheckCompleted();
        }

        private bool CheckCompleted()
        {
            IsCompleted = Steps.Count == 0;
            return IsCompleted;
        }

        private int CheckStepIsCompleted(int index)
        {
            if (Steps[index].IsCompleted)
            {
                Steps.RemoveAt(index);
            }
            else
            {
                index++;
            }

            return index;
        }
    }
}
